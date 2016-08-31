using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;

namespace DontPanic.Helpers
{
    [ErrorHandlerBehavior]
    [AutoConfigure]
    public abstract class ServiceBase
    {
        private IProxyFactory _factory;

        public virtual IProxyFactory Factory
        {
            get
            {
                if (_factory == null)
                {
                    _factory = CreateFactory();
                }
                return _factory;
            }
            set
            {
                _factory = value;
            }
        }

        protected virtual IProxyFactory CreateFactory()
        {
            return new ProxyFactory();
        }
    }

    public interface IServiceHelperAttribute
    {
        // service
        void ConfigureService(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase);

        // client / proxy
        EndpointIdentity CreateIdentity();
        void ConfigureClientBinding(Binding binding, Type contractType);
        void ConfigureClientCredentials(ClientCredentials creds, Type contractType);
    }

    class DefaultServiceHelperAttribute : IServiceHelperAttribute
    {
        public void ConfigureService(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {

        }

        public EndpointIdentity CreateIdentity()
        {
            return null;
        }

        public void ConfigureClientBinding(Binding binding, Type contractType)
        {

        }

        public void ConfigureClientCredentials(ClientCredentials creds, Type contractType)
        {

        }
    }

}
