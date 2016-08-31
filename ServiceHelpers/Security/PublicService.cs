using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Diagnostics;

namespace DontPanic.Helpers
{
    [AttributeUsage(AttributeTargets.Interface)] 
    public class PublicService : Attribute, IServiceHelperAttribute
    {
        public PublicService()
        {

        }

        public void ConfigureService(System.ServiceModel.Description.ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            var host = (serviceHostBase as ServiceHost);
            if (host != null)
            {
                // Don't use this crappy binding
                var address = serviceHostBase.Description.Endpoints.First().Address;
                var contract = serviceHostBase.Description.Endpoints.First().Contract;

                // clear existing
                host.Description.Endpoints.Clear();

                var binding = new WS2007HttpBinding();
                binding.Security.Mode = SecurityMode.None;
                
                MaxSetter.SetMaxes(binding);

                host.AddServiceEndpoint(contract.ContractType, binding, address.Uri);
            }

            DisableErrorMasking.Disable(serviceHostBase);
            
        }

        public System.ServiceModel.EndpointIdentity CreateIdentity()
        {
            return null;
        }

        public void ConfigureClientBinding(System.ServiceModel.Channels.Binding binding, Type contractType)
        {
            if (binding is WS2007HttpBinding)
            {                
                (binding as WS2007HttpBinding).Security.Mode = SecurityMode.None;
            }
        }

        public void ConfigureClientCredentials(System.ServiceModel.Description.ClientCredentials creds, Type contractType)
        {
            
        }
    }
}
