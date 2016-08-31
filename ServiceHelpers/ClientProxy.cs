using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.ServiceModel.Description;
using System.Diagnostics;

namespace DontPanic.Helpers
{
    public class ClientProxy<I> : IChannelWrapper<I> where I : class
    {
        ChannelFactory<I> _factory;
        I _channel;

        /// <summary>
        /// Used when calling out of process WCF services.
        /// Aka any WCF service that is configured in the web.config or app.config.
        /// </summary>
        public ClientProxy()
        {
            _factory = new ChannelFactory<I>();
        }

        public ClientProxy(string configName)
        {
            _factory = new ChannelFactory<I>(configName);

        }

        /// <summary>
        /// Used when calling in process
        /// </summary>
        public ClientProxy(Binding binding, EndpointAddress address)            
        {
            _factory = new ChannelFactory<I>(binding, address);
        }

        public I Instance
        {
            get
            {
                if (_channel == null)
                {
                    _channel = _factory.CreateChannel();
                }
                return _channel;
            }
        }

        public bool IsRealProxy
        {
            get { return true; }
        }

        public int CacheCount
        {
            get;
            set;
        }

        private string _cacheKey;
        public string CacheKey
        {
            get
            {
                if (_cacheKey == null)
                    _cacheKey = typeof(I).FullName;
                return _cacheKey;
            }            
        }

        public ClientCredentials ClientCredentials
        {
            get
            {
                return _factory.Credentials;
            }
        }

        public bool InUse
        {
            get;
            set;
        }

        public void Close()
        {
            _factory.Close();
        }

        public void Abort()
        {
            _factory.Abort();
        }

        public DateTime LastUse { get; set; }

        public string ChannelType { get { return typeof(I).Name; } }

        public bool IsFaulted
        {
            get
            {
                const int MaxCacheMinutes = 5;
                var result = _factory.State == CommunicationState.Faulted || LastUse < DateTime.Now.AddMinutes(-MaxCacheMinutes);
                return result;
            }
        }
    }

}
