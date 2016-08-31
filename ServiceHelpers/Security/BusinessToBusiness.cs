using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Security;
using System.ServiceModel;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;

namespace DontPanic.Helpers
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class BusinessToBusiness : Attribute, IServiceHelperAttribute
    {
        public BusinessToBusiness(string clientCert, string serviceCert)
        {
            ClientCert = clientCert;
            ServiceCert = serviceCert;
            ValidationMode = X509CertificateValidationMode.PeerTrust; // default to peer trust.
        }

        public BusinessToBusiness(string clientCert, string serviceCert, X509CertificateValidationMode validationMode)
        {
            ClientCert = clientCert;
            ServiceCert = serviceCert;
            ValidationMode = validationMode;
        }

        public string ClientCert { get; set; }
        public string ServiceCert { get; set; }
        public X509CertificateValidationMode ValidationMode { get; set; }

        public void ConfigureService(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (ConfigHelper.IsSecurityEnabled(serviceHostBase))
            {
                // Get certificate location from the cert store.
                StoreLocation location = StoreLocation.LocalMachine;
                CertHelper.TryGetCertLocation(ServiceCert, out location, true);

                // Set certificate
                
                serviceHostBase.Credentials.ServiceCertificate.SetCertificate(
                    location,
                    System.Security.Cryptography.X509Certificates.StoreName.My,
                    X509FindType.FindBySubjectName,
                    ServiceCert);

                // Set certificate validation mode (defaults to peer trust).
                serviceHostBase.Credentials.ClientCertificate.Authentication.CertificateValidationMode = ValidationMode;

                if (serviceHostBase.Description.Endpoints != null)
                {
                    bool reConfigure = false;
                    foreach (var endpoint in serviceHostBase.Description.Endpoints)
                    {
                        if (endpoint.Binding is WS2007HttpBinding)
                        {
                            // Setup each endpoint to use Message security. 
                            var binding = endpoint.Binding as WS2007HttpBinding;
                            binding.Security.Mode = SecurityMode.Message;
                            binding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                            binding.Security.Message.EstablishSecurityContext = false;
                            binding.Security.Message.NegotiateServiceCredential = false;
                            MaxSetter.SetMaxes(binding);                            
                        }
                        if (endpoint.Binding is NetTcpBinding)
                        {
                            // Setup each endpoint to use Message security. 
                            var binding = endpoint.Binding as NetTcpBinding;
                            binding.Security.Mode = SecurityMode.Message;
                            binding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                            MaxSetter.SetMaxes(binding);
                        }
                        if (endpoint.Binding is BasicHttpBinding)
                        {
                            reConfigure = true;
                        }                        
                    }                    

                    // reconfigure host
                    if (reConfigure && serviceHostBase.Description.Endpoints.Count() > 0)
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
                            binding.Security.Mode = SecurityMode.Message;
                            binding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                            binding.Security.Message.EstablishSecurityContext = false;
                            
                            MaxSetter.SetMaxes(binding);

                            var endpoint = host.AddServiceEndpoint(contract.ContractType, binding, address.Uri);                                                       
                        }
                    }
                    
                    DisableErrorMasking.Disable(serviceHostBase);                    
                }
            }
        }

        public EndpointIdentity CreateIdentity()
        {
            return new DnsEndpointIdentity(ServiceCert);
        }

        public void ConfigureClientBinding(Binding binding, Type contractType)
        {
            if (binding is WS2007HttpBinding)
            {
                var wsBinding = binding as WS2007HttpBinding;
                wsBinding.Security.Mode = SecurityMode.Message;
                wsBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                wsBinding.Security.Message.EstablishSecurityContext = false;
                wsBinding.Security.Message.NegotiateServiceCredential = false;                               
            }
            if (binding is NetTcpBinding)
            {
                (binding as NetTcpBinding).Security.Mode = SecurityMode.Message;
                (binding as NetTcpBinding).Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
            }
        }

        public void ConfigureClientCredentials(ClientCredentials creds, Type contractType)
        {
            if (ConfigHelper.IsSecurityEnabled(contractType))
            {
                StoreLocation location = StoreLocation.LocalMachine;
                CertHelper.TryGetCertLocation(ClientCert, out location, true);

                creds.ClientCertificate.SetCertificate(
                    location,
                    System.Security.Cryptography.X509Certificates.StoreName.My,
                    X509FindType.FindBySubjectName,
                    ClientCert);

                creds.ServiceCertificate.Authentication.CertificateValidationMode = ValidationMode;

                StoreLocation serviceLocation = StoreLocation.LocalMachine;
                CertHelper.TryGetCertLocation(ServiceCert, out serviceLocation, true);

                creds.ServiceCertificate.SetDefaultCertificate(
                    serviceLocation,
                    System.Security.Cryptography.X509Certificates.StoreName.My,
                    X509FindType.FindBySubjectName,
                    ServiceCert);                    
            }
        }
    }

}
