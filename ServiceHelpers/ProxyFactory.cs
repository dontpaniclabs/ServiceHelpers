using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.Web.Configuration;
using System.Reflection;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;

namespace DontPanic.Helpers
{
    public delegate void WcfCall<I>(I proxyInterface);
    public delegate IChannelWrapper<I> CreateWcfChannel<I>();

    /// <summary>
    /// Used to create instances of channel wrappers. Both in proc and real.
    /// </summary>
    public class ProxyFactory : IProxyFactory
    {
        #region Mock support for easy unit testing

        private Dictionary<string, object> _overrides = new Dictionary<string, object>();
        private Dictionary<string, Uri> _endpointOverride = new Dictionary<string, Uri>();

        private MockChannelWrapper<I> GetProxyOverride<I>() where I : class
        {
            string key = typeof(I).FullName;

            if (_overrides.ContainsKey(key))
            {
                I resultInterface = _overrides[key] as I;
                if (resultInterface != null)
                {
                    return new MockChannelWrapper<I>() { Instance = resultInterface };
                }
            }

            return null;
        }

        #endregion

        #region Current public interface

        /// <summary>
        /// Override default proxy implementation
        /// </summary>        
        public void AddProxyOverride<I, T>(T proxy)
            where T : I
            where I : class
        {
            AddProxyOverride<I>(proxy);
        }

        /// <summary>
        /// Override default proxy implementation
        /// </summary>
        public void AddProxyOverride<I>(I proxy)
            where I : class
        {
            string key = typeof(I).FullName;

            if (_overrides.ContainsKey(key))
            {
                _overrides.Remove(key);
            }
            _overrides.Add(key, proxy);

            if (proxy != null)
            {
                var prop = proxy.GetType().GetProperty("Factory");
                if (prop != null && prop.CanRead && prop.CanWrite)
                {
                    prop.SetValue(proxy, this, new object[] { });
                }
            }
        }

        public void ClearProxyOverrides()
        {
            _overrides.Clear();
        }

        public virtual void AddEndpointAddressOverride<I>(Uri uri)
        {
            // first we must clear the cache of anything for this proxy type.
            var key = typeof(I).FullName;

            lock (_lockObj)
            {
                if (_endpointOverride.ContainsKey(key))
                {
                    _endpointOverride.Clear();
                }

                _endpointOverride.Add(key, uri);
            }
        }

        public virtual void ClearEndpointOverrides()
        {
            lock (_lockObj)
            {
                _endpointOverride.Clear();
            }
        }

        /// <summary>
        /// Call a method on a WCF proxy. 
        /// This method is used by the generated proxy.
        /// The generated proxy will call this method for each call thru the proxy.
        /// </summary>
        public virtual object CallMethod<I>(string name, object[] parameters, string endpointOverrideAddress) where I : class
        {
            object result = null;

            LogCall("ProxyFactory:CallMethod - " + name);

            using (var perfTrace = new PerfTrace(typeof(I), name))
            {
                if (!MessageFilterHelper.PreFilter<I>(this, name, parameters, ref result))
                {
                    Call<I>(proxy =>
                    {
                        var method = GetMethod(proxy.GetType(), name);
                        result = method.Invoke(proxy, parameters);
                    }, endpointOverrideAddress);

                    MessageFilterHelper.PostFilter<I>(this, name, parameters, ref result);
                }
            }

            return result;
        }

        /// <summary>
        /// Call WCF using a dynamic proxy
        /// </summary>
        public virtual I Proxy<I>() where I : class
        {
            return Proxy<I>(null);
        }

        public virtual I Proxy<I>(string endpointOverrideAddress) where I : class
        {
            var obj = Proxy(typeof(I), endpointOverrideAddress);
            return obj as I;
        }


        /// <summary>
        /// Internal use only. We might expose this method someday, but not for now.
        /// With good reason we could expose it, but we haven't seen that reason yet.
        /// </summary>
        protected virtual object Proxy(Type contractType, string endpointOverrideAddress)
        {
            Type proxyType = ProxyEmitter.EmitProxy("CallMethod", contractType, endpointOverrideAddress);
            var obj = Activator.CreateInstance(proxyType);
            obj.GetType().GetField("Factory").SetValue(obj, this);
            return obj;
        }

        /// <summary>
        /// Proxy a call to WCF
        /// </summary>
        public virtual void Call<I>(WcfCall<I> call) where I : class
        {
            Call<I>(call, null);
        }

        /// <summary>
        /// Proxy a call to WCF
        /// </summary>
        public virtual void Call<I>(WcfCall<I> call, string endpointOverrideAddress) where I : class
        {
            IChannelWrapper<I> wrapper = null;
            bool abort = true;

            try
            {
                bool skipCall = false;
                wrapper = CreateProxy<I>(endpointOverrideAddress);
                if (wrapper.IsRealProxy && wrapper.CacheCount > 0)
                {
                    // retry one time
                    try
                    {
                        // turn off every call logging
                        //Log(string.Format("Calling {0} with cached proxy", typeof(I).Name));
                        call.Invoke(wrapper.Instance);
                        skipCall = true;
                    }
                    catch (Exception ex)
                    {
                        LogException("First call exception with a cached proxy", ex);
                        CloseProxy<I>(wrapper, true);
                        wrapper = CreateProxy<I>(endpointOverrideAddress);
                    }
                }

                if (!skipCall)
                {
                    call.Invoke(wrapper.Instance);
                    abort = false;
                }

                if (wrapper != null)
                    wrapper.LastUse = DateTime.Now;
            }
            catch (CommunicationException cex)
            {
                if (HandleCommunicationException<I>(cex, wrapper) == HandleExceptionAction.ThrowException)
                    throw;
            }
            catch (TimeoutException tex)
            {
                if (HandleTimeoutException<I>(tex, wrapper) == HandleExceptionAction.ThrowException)
                    throw;
            }
            catch (Exception ex)
            {
                if (HandleException<I>(ex, wrapper) == HandleExceptionAction.ThrowException)
                    throw;
            }
            finally
            {
                if (wrapper != null)
                {
                    try
                    {
                        CloseProxy<I>(wrapper, abort);
                    }
                    catch
                    { }
                }
            }
        }

        #endregion

        #region Internal

        private static Dictionary<string, List<ICachableChannel>> _proxyCache = new Dictionary<string, List<ICachableChannel>>();
        private static object _lockObj = new object();

        protected virtual IChannelWrapper<I> CreateProxy<I>(string endpointOverrideAddress)
    where I : class
        {
            IChannelWrapper<I> result = null;

            var key = typeof(I).FullName;

            if (_proxyCache.ContainsKey(key))
            {
                lock (_lockObj)
                {
                    var cache = _proxyCache[key];

                    cache.RemoveAll(p => p.IsFaulted);

                    if (cache.Where(p => p.InUse == false).Count() > 0)
                    {
                        result = cache.First() as IChannelWrapper<I>;
                        result.InUse = true;
                    }
                }
            }

            if (result == null)
            {
                result = CreateProxyInternal<I>(endpointOverrideAddress);
                if (result.Instance != null)
                {
                    var factoryProp = result.Instance.GetType().GetProperty("Factory");
                    if (factoryProp != null && factoryProp.PropertyType.Name == typeof(IProxyFactory).Name)
                    {
                        factoryProp.SetValue(result.Instance, this, new object[] { });
                    }
                }
                result.InUse = true;
            }

            return result;
        }

        protected virtual void CloseProxy<I>(IChannelWrapper<I> proxy, bool abort)
        {
            var key = typeof(I).FullName;

            if (abort)
            {
                proxy.Abort();
            }

            if (proxy.CacheCount > 0)
            {
                lock (_lockObj)
                {
                    if (!_proxyCache.ContainsKey(key))
                    {
                        _proxyCache.Add(key, new List<ICachableChannel>());
                    }

                    var cache = _proxyCache[key];
                    if (cache.Where(p => p.CacheKey == key).Count() > 0)
                    {
                        // found in cache
                        var proxyCached = cache.Where(p => p.CacheKey == key).First();
                        proxy.InUse = false;

                        if (abort)
                            cache.Remove(proxyCached);
                    }
                    else if (!abort)
                    {
                        // not in cache and not aborted
                        if (proxy.CacheCount > cache.Count)
                        {
                            proxy.InUse = false;
                            cache.Add(proxy);
                        }
                        else
                        {
                            // no room to cache, kill it
                            try
                            {
                                proxy.Close();
                            }
                            catch
                            {
                                proxy.Abort();
                            }
                        }
                    }
                }
            }
            else if (!abort)
            {
                // never cache this proxy
                try
                {
                    proxy.Close();
                }
                catch
                {
                    proxy.Abort();
                }
            }
        }

        protected virtual IChannelWrapper<I> CreateProxyInternal<I>(string endpointOverrideAddress)
            where I : class
        {
            // if they override type, just give them the override back
            var mock = GetProxyOverride<I>();
            if (mock != null)
                return mock;

            // look into actually generating a proxy
            Type serviceType = null;
            bool noWcf = false;
            int cacheCount = 0;

            if (endpointOverrideAddress != null)
            {
                return ConfigureEndpointFromUri<I>(new Uri(endpointOverrideAddress));
            }

            if (_endpointOverride.ContainsKey(typeof(I).FullName))
            {
                return ConfigureEndpointFromUri<I>(_endpointOverride[typeof(I).FullName]);
            }

            string configName;
            if (IsEndpointServiceModelConfigured<I>(out configName))
            {
                // Let default behavior take over.
                return new ClientProxy<I>(configName);
            }

            // load from confgiuration
            if (serviceType == null)
            {
                if (ServiceHelpersConfigSection.Settings != null)
                {
                    var endpoint = ServiceHelpersConfigSection.Settings.Endpoint(typeof(I));

                    if (endpoint != null)
                    {
                        return ConfigureEndpointFromSetting<I>(endpoint);
                    }
                }
            }

            // load from configuration (old style)
            if (serviceType == null)
            {
                var typeName = typeof(I).Name + "_Load";
                var configuredType = ConfigurationManager.AppSettings[typeName];
                if (configuredType != null)
                    serviceType = Type.GetType(configuredType);
                else
                {
                    typeName = typeof(I).FullName + "_Load";
                    configuredType = ConfigurationManager.AppSettings[typeName];
                    if (configuredType != null)
                        serviceType = Type.GetType(configuredType);
                }
            }

            if (serviceType == null)
            {
                // check attribute
                var attribute = typeof(I).GetCustomAttributes(typeof(InProcAttribute), true);
                if (attribute != null && attribute.Length > 0)
                {
                    serviceType = (attribute.First() as InProcAttribute).ServiceType;
                    noWcf = (attribute.First() as InProcAttribute).NoWcf;
                    cacheCount = (attribute.First() as InProcAttribute).CacheCount;
                }
            }

            if (serviceType == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                    "No configuration for {0}. Configure in app.config or " +
                    "web.config or apply the InProc attribute to the contract.",
                    typeof(I).Name));
            }

            // If WCF is not enabled, than just create a mock channel wrapper
            if (!EnableInProc || noWcf)
            {
                return new MockChannelWrapper<I>() { Instance = Activator.CreateInstance(serviceType) as I };
            }

            // Create wrapper for WCF in proc service.
            var wrapper = CreateInProcWrapper<I>(serviceType);
            wrapper.CacheCount = cacheCount;
            return wrapper;
        }

        /// <summary>
        /// Was this endpoint configured using ServiceModel
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <returns></returns>
        protected virtual bool IsEndpointServiceModelConfigured<I>(out string itemName)
        {
            bool result = false;
            itemName = string.Empty;

            try
            {
                ClientSection clientSection =
                    ConfigurationManager.GetSection("system.serviceModel/client") as ClientSection;

                if (clientSection != null)
                {
                    foreach (ChannelEndpointElement endpoint in clientSection.Endpoints)
                    {
                        if (endpoint.Contract == typeof(I).FullName)
                        {
                            itemName = endpoint.Name;
                            result = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return result;
        }

        protected virtual IChannelWrapper<I> ConfigureEndpointFromUri<I>(Uri uri)
            where I : class
        {
            var clientEndpoint = new ClientEndpoint();
            clientEndpoint.Address = uri.ToString();

            return ConfigureEndpointFromSetting<I>(clientEndpoint);
        }

        /// <summary>
        /// Configure an endpoint from a setting
        /// </summary>        
        protected virtual IChannelWrapper<I> ConfigureEndpointFromSetting<I>(ClientEndpoint endpoint)
            where I : class
        {
            IChannelWrapper<I> result = null;

            if (!string.IsNullOrWhiteSpace(endpoint.Implementation))
            {
                Type t = Type.GetType(endpoint.Implementation);
                if (t == null)
                    throw new InvalidOperationException(
                        string.Format(
                        "Unable to create service type, your configuration for endpoint.Implementation is probably wrong or you need to add a reference to an assembly. Configured as {0}.",
                        endpoint.Implementation));

                if (endpoint.UseWcf)
                {
                    result = CreateInProcWrapper<I>(t);
                }
                else
                {
                    result = new MockChannelWrapper<I>() { Instance = Activator.CreateInstance(t) as I };
                }
            }
            else if (!string.IsNullOrWhiteSpace(endpoint.Address))
            {
                if (string.IsNullOrWhiteSpace(endpoint.ChannelFactory))
                {
                    IServiceHelperAttribute configureHelper = new DefaultServiceHelperAttribute();

                    Binding binding = null;
                    ClientProxy<I> proxy = null;
                    EndpointIdentity identity = null;

                    var attributes = typeof(I).GetCustomAttributes(false);
                    foreach (var attribute in attributes)
                    {
                        if (attribute is IServiceHelperAttribute)
                        {
                            configureHelper = attribute as IServiceHelperAttribute;
                            identity = configureHelper.CreateIdentity();
                        }
                    }

                    if (endpoint.Address.StartsWith("net.tcp://"))
                    {
                        binding = new NetTcpBinding()
                        {
                            MaxBufferPoolSize = int.MaxValue,
                            MaxBufferSize = int.MaxValue,
                            MaxReceivedMessageSize = int.MaxValue,
                        };
                        MaxSetter.SetMaxes(binding);
                    }
                    else if (endpoint.Address.StartsWith("http://") || endpoint.Address.StartsWith("https://"))
                    {
                        binding = new WS2007HttpBinding(SecurityMode.None)
                        {
                            MaxBufferPoolSize = int.MaxValue,
                            MaxReceivedMessageSize = int.MaxValue,
                        };
                        MaxSetter.SetMaxes(binding);
                        MaxSetter.SetTimeouts<I>(binding);
                    }
                    else if (endpoint.Address.StartsWith("net.msmq://"))
                    {
                        binding = new NetMsmqBinding()
                        {
                            MaxBufferPoolSize = int.MaxValue,
                            MaxReceivedMessageSize = int.MaxValue,
                        };
                    }
                    else if (endpoint.Address.StartsWith("net.pipe://"))
                    {
                        binding = new NetNamedPipeBinding()
                        {
                            MaxBufferPoolSize = int.MaxValue,
                            MaxReceivedMessageSize = int.MaxValue,
                        };
                    }
                    configureHelper.ConfigureClientBinding(binding, typeof(I));

                    proxy = new ClientProxy<I>(
                        binding,
                        new EndpointAddress(new Uri(endpoint.Address), configureHelper.CreateIdentity()));

                    configureHelper.ConfigureClientCredentials(proxy.ClientCredentials, typeof(I));

                    result = proxy;
                }
                else
                {
                    var cfType = Type.GetType(endpoint.ChannelFactory);
                    if (cfType == null)
                        throw new ConfigurationErrorsException("ChannelFactory is invalid");

                    var cf = Activator.CreateInstance(cfType) as ICustomChannelFactory;

                    if (cf == null)
                        throw new ConfigurationErrorsException("Could not create custom channel factory.");

                    result = cf.CreateChannel<I>(endpoint);
                }
            }

            if (result != null)
            {
                result.CacheCount = endpoint.CacheCount;
            }

            return result;
        }

        protected virtual ClientProxy<I> CreateInProcWrapper<I>(Type serviceType)
            where I : class
        {
            // We are using WCF, create host.
            // InProcHost.CreateHost will cache the host.
            var url = InProcessHost.CreateHost(serviceType, typeof(I));

            var binding = new NetNamedPipeBinding()
            {
                MaxBufferPoolSize = int.MaxValue,
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
            };

            MaxSetter.SetMaxes(binding);

            return new ClientProxy<I>(
                binding,
                new EndpointAddress(url));
        }

        /// <summary>
        /// Check to see if in proc is enabled. 
        /// Defaults to enabled
        /// </summary>
        private static bool EnableInProc
        {
            get
            {
                bool result = true;

                if (ServiceHelpersConfigSection.Settings != null)
                {
                    result = ServiceHelpersConfigSection.Settings.EnableInProc;
                }
                else
                {
                    const string configKey = "DontPanic.ServiceHelper.DisableInProc";
                    string config = ConfigurationManager.AppSettings[configKey];

                    if (!string.IsNullOrWhiteSpace(config))
                    {
                        if (config.ToLower() == "true")
                        {
                            result = false;
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Retrieve method for an inteface using reflection.
        /// </summary>
        private static MethodInfo GetMethod(Type interfaceType, string name)
        {
            MethodInfo result = interfaceType.GetMethod(name);
            if (result == null)
            {
                var interfaces = interfaceType.GetInterfaces();
                if (interfaces != null)
                {
                    foreach (var subInterface in interfaces)
                    {
                        result = GetMethod(subInterface, name);
                        if (result != null)
                            break;
                    }
                }
            }
            return result;
        }


        #endregion

        #region Logging

        protected virtual void Log(string message)
        {
            if (LoggerCache.Logger != null && LogEnabled)
            {
                LoggerCache.Logger.Log(message);
            }
        }

        protected virtual void LogCall(string message)
        {
            if (LoggerCache.Logger != null && LogEnabled)
            {
                LoggerCache.Logger.LogCall(message);
            }
        }

        protected virtual void LogException(string message, Exception ex)
        {
            if (LoggerCache.Logger != null && LogEnabled)
            {
                LoggerCache.Logger.LogException(message, ex);
            }
        }

        private bool _logEnabled = false;

        public bool LogEnabled
        {
            get 
            { 
                return _logEnabled || (ServiceHelpersConfigSection.Settings != null && ServiceHelpersConfigSection.Settings.Log); 
            }
            set { _logEnabled = value; }
        }

        #endregion

        #region Exception Handling

        public enum HandleExceptionAction
        {
            ThrowException,
            DoNotThrowException
        }

        protected virtual HandleExceptionAction HandleCommunicationException<I>(CommunicationException ex, IChannelWrapper<I> wrapper)
        {
            LogException("Communication exception while proxying a call. ", ex);
            return HandleExceptionAction.ThrowException;
        }

        protected virtual HandleExceptionAction HandleTimeoutException<I>(TimeoutException ex, IChannelWrapper<I> wrapper)
        {
            LogException("Timeout exception while proxying a call. ", ex);
            return HandleExceptionAction.ThrowException;
        }

        protected virtual HandleExceptionAction HandleException<I>(Exception ex, IChannelWrapper<I> wrapper)
        {
            var result = HandleExceptionAction.DoNotThrowException;

            if (ex is TargetInvocationException && ex.InnerException != null)
            {
                LogException("Unhandled exception while proxying a call. ", ex.InnerException);

                if (wrapper.IsRealProxy)
                {
                    if (ex.InnerException is FaultException<ServiceError>)
                    {
                        // WCF is on and our custom ErrorHandlerBehavior already created a custom FaultException<ServiceError>
                        throw ex.InnerException;
                    }
                    else
                    {
                        result = HandleExceptionAction.ThrowException;
                    }
                }
                else
                {
                    // WCF is off, we need to 
                    if (ServiceHelpersConfigSection.Settings == null || ServiceHelpersConfigSection.Settings.CatchNonWcfFaults)
                    {
                        if (ex.InnerException is FaultException<ServiceError>)
                        {
                            // Our custom ErrorHandlerBehavior already created a custom FaultException<ServiceError>
                            throw ex.InnerException;
                        }
                        else
                        {
                            ServiceError serviceError = new ServiceError();
                            serviceError.ExceptionType = ex.InnerException.GetType().ToString();
                            serviceError.ExceptionMessage = ex.InnerException.Message;
                            serviceError.ExceptionDetail = ex.InnerException.ToString();
                            FaultException<ServiceError> faultEx = new FaultException<ServiceError>(serviceError,
                                "Unhandled exception in WCF service.",
                                FaultCode.CreateSenderFaultCode("Unhandled Exception", "DPL"));
                            throw faultEx;
                        }
                    }
                    else
                    {
                        throw ex.InnerException;
                    }
                }
            }
            else
            {
                result = HandleExceptionAction.ThrowException;
            }
            return result;
        }

        #endregion

    }
}
