using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;

namespace DontPanic.Helpers
{
    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class ExceptionBehaviorAttribute : Attribute, IContractBehavior
    {
        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            //throw new NotImplementedException();
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            //throw new NotImplementedException();
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime)
        {
            //throw new NotImplementedException();
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
            //throw new NotImplementedException();
            ApplyErrorBehavior(contractDescription, endpoint);
        }

        private void ApplyErrorBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
            foreach (OperationDescription opDesc in contractDescription.Operations)
            {
                FaultDescription faultDescription = new FaultDescription(endpoint.Contract.Namespace + endpoint.Contract.Name + "/" + opDesc.Name + "ServiceErrorFault");
                faultDescription.DetailType = typeof(ServiceError);
                opDesc.Faults.Add(faultDescription);
            }
        }
    }
}
