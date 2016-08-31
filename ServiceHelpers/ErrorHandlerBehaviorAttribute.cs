using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.Collections.ObjectModel;
using System.Reflection;

namespace DontPanic.Helpers
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ErrorHandlerBehaviorAttribute : Attribute, IServiceBehavior, IErrorHandler
    {
        private Type ServiceType { get; set; }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            ServiceType = serviceDescription.ServiceType;
            foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
            {
                dispatcher.ErrorHandlers.Add(this);
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        public bool HandleError(Exception error)
        {
            LoggerCache.Logger.LogException("Unhandled exception in WCF service", error);
            return false;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (error is FaultException)
            {
                return;
            }

            if (ServiceHelpersConfigSection.Settings == null || ServiceHelpersConfigSection.Settings.CatchNonWcfFaults)
            {
                // create our custom FaultException<ServiceError>
                ServiceError serviceError = new ServiceError();
                serviceError.Id = Guid.NewGuid();
                serviceError.ExceptionType = error.GetType().ToString();
                serviceError.ExceptionMessage = error.Message;
                string detail = "Exception detail not available.";
#if DEBUG
            // only include exception detail in debug builds
            detail = error.ToString();
#else
                detail = string.Empty;
#endif
                serviceError.ExceptionDetail = detail;

                FaultException<ServiceError> faultEx = new FaultException<ServiceError>(serviceError,
                    "Unhandled exception in WCF service.",
                    FaultCode.CreateSenderFaultCode("Unhandled Exception", "DPL"));
                MessageFault messageFault = faultEx.CreateMessageFault();
                fault = Message.CreateMessage(version, messageFault, faultEx.Action);
            }
        }


    }
}
