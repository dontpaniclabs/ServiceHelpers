using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DontPanic.Helpers.Security;
using System.Security.Cryptography.X509Certificates;
using DontPanic.Helpers;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace CloudSSLHost
{
    public class MySslFactory : IManualServiceFactoryBase
    {
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
                binding.Security.Mode = SecurityMode.Transport;
                binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
                binding.Security.Message.EstablishSecurityContext = false;
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

                MaxSetter.SetMaxes(binding);

                if (address.Uri.ToString().Contains("http://"))
                    address = new EndpointAddress(new Uri(address.Uri.ToString().Replace("http://", "https://")));
                var endpoint = host.AddServiceEndpoint(contract.ContractType, binding, address.Uri);

                host.Description.Behaviors.Add(new ServiceMetadataBehavior() { HttpGetEnabled = true, HttpsGetEnabled = true });
            }

            DisableErrorMasking.Disable(serviceHostBase);
        }         
    }

    public class MySslFactoryWithMessageCred : IManualServiceFactoryBase
    {
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
                binding.Security.Mode = SecurityMode.TransportWithMessageCredential;
                binding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                binding.Security.Message.EstablishSecurityContext = false;
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

                MaxSetter.SetMaxes(binding);

                if (address.Uri.ToString().Contains("http://"))
                    address = new EndpointAddress(new Uri(address.Uri.ToString().Replace("http://", "https://")));
                var endpoint = host.AddServiceEndpoint(contract.ContractType, binding, address.Uri);

                host.Description.Behaviors.Add(new ServiceMetadataBehavior() { HttpGetEnabled = true, HttpsGetEnabled = true });
            }

            DisableErrorMasking.Disable(serviceHostBase);
        }
    }
}