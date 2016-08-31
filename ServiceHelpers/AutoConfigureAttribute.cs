using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;

namespace DontPanic.Helpers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoConfigureAttribute : Attribute, IServiceBehavior
    {
        public void AddBindingParameters(ServiceDescription serviceDescription,
            System.ServiceModel.ServiceHostBase serviceHostBase,
            System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints,
            System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {            
        }

        public void Validate(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {            
            var interfaces = serviceDescription.ServiceType.GetInterfaces();
            if (interfaces != null)
            {
                foreach (var endpoint in interfaces)
                {                    
                    var attributes = endpoint.GetCustomAttributes(true);
                    if (attributes != null)
                    {
                        foreach (var attribute in attributes)
                        {
                            if (attribute is IServiceHelperAttribute)
                            {
                                (attribute as IServiceHelperAttribute).ConfigureService(serviceDescription, serviceHostBase);
                            }
                        }
                    }
                }
            }
        }
    }
}
