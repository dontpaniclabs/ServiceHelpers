using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using DontPanic.Helpers;

namespace WorkerRole1
{
    [ServiceContract]
    [BusinessToBusiness("RawTcpClientCert1", "RawTcpServiceCert1")]
    public interface IMyService
    {
        [OperationContract]
        string TestMe(string input);
    }

    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public class MyService : ServiceBase, IMyService
    {
        public string TestMe(string input)
        {
            return input;
        }
    }
}
