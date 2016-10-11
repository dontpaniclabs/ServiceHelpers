using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace DontPanic.Helpers.Security
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class SSLOnly : Attribute, IServiceHelperAttribute
    {
        public void ConfigureService(System.ServiceModel.Description.ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            var host = (serviceHostBase as ServiceHost);
            if (host != null)
            {
                var address = serviceHostBase.Description.Endpoints.First().Address;
                var contract = serviceHostBase.Description.Endpoints.First().Contract;

                if (address.ToString().ToLower().Contains("http"))
                {
                    // clear existing
                    host.Description.Endpoints.Clear();

                    // Use HTTPS (SSL) for communication 
                    var binding = new WS2007HttpBinding(SecurityMode.Transport);
                    //binding.Security.Mode = SecurityMode.Transport;
                    binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;                  

                    MaxSetter.SetMaxes(binding);

                    if (address.Uri.ToString().Contains("http://"))
                    {
                        var uri = address.Uri.ToString().Replace("http://", "https://");
                        if (ServiceHelpersConfigSection.Settings != null && ServiceHelpersConfigSection.Settings.SSLPort > 0)
                            uri = uri.Replace(address.Uri.Port.ToString(), ServiceHelpersConfigSection.Settings.SSLPort.ToString());
                        address = new EndpointAddress(new Uri(uri));
                    }
                    var endpoint = host.AddServiceEndpoint(contract.ContractType, binding, address.Uri);
                }

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
                var bindingWS = binding as WS2007HttpBinding;

                // Use transport with message creds                            
                bindingWS.Security.Mode = SecurityMode.Transport;

                if (ServiceHelpersConfigSection.Settings.Endpoint(contractType).WindowsAuth)
                    bindingWS.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
                else
                    bindingWS.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

                MaxSetter.SetMaxes(bindingWS);
            }            
        }

        public void ConfigureClientCredentials(System.ServiceModel.Description.ClientCredentials creds, Type contractType)
        {
            
        }
    }

    public class SLLBehavior : IEndpointBehavior, IDispatchMessageInspector
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            var caller = "";
            if (ServiceSecurityContext.Current != null && ServiceSecurityContext.Current.WindowsIdentity != null)
            {
                // get from WCF (caller)
                caller = ServiceSecurityContext.Current.WindowsIdentity.Name;
            }
            if (string.IsNullOrWhiteSpace(caller)
                && HttpContext.Current != null
                && HttpContext.Current.Request != null
                && HttpContext.Current.Request.LogonUserIdentity != null)
            {
                // get from IIS (caller)
                caller = HttpContext.Current.Request.LogonUserIdentity.Name;
            }
            return null;   
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }
}
