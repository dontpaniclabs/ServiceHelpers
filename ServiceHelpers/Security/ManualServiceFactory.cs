using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace DontPanic.Helpers.Security
{
    /// <summary>
    /// Using ManualServiceFactory allows for easy managing how what occurs when a service is created.
    /// This can be used to add security when creating your services.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ManualServiceFactoryAttribute : Attribute, IServiceHelperAttribute
    {
        Type _type;
        string _typeString;
        IManualServiceFactoryBase _factory;

        public ManualServiceFactoryAttribute(string typeString)
        {
            _typeString = typeString;
        }

        public ManualServiceFactoryAttribute(Type type)            
        {
            _type = type;
        }

        public Type ServiceFactory
        {
            get
            {
                if (_type == null && !string.IsNullOrWhiteSpace(_typeString))
                {
                    _type = Type.GetType(_typeString);
                }
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public IManualServiceFactoryBase Factory
        {
            get
            {
                if (_factory == null)
                {
                    if (ServiceFactory == null)
                        throw new InvalidOperationException("ServiceFactory type is null. Please specify a service factory type. " + _typeString);

                    _factory = Activator.CreateInstance(ServiceFactory) as IManualServiceFactoryBase;
                    if (_factory == null)
                        throw new InvalidOperationException("Unable to create ServiceFactory from type " + ServiceFactory.FullName + ".");
                }
                return _factory;
            }
        }

        public void ConfigureService(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            Factory.ConfigureService(serviceDescription, serviceHostBase);
        }

        public EndpointIdentity CreateIdentity()
        {
            return null;
        }

        public void ConfigureClientBinding(System.ServiceModel.Channels.Binding binding, Type contractType)
        {
            
        }

        public void ConfigureClientCredentials(ClientCredentials creds, Type contractType)
        {
            
        }
    }

    public interface IManualServiceFactoryBase
    {
        void ConfigureService(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase);        
    }
}
