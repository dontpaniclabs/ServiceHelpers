using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using DontPanic.Helpers;
using DontPanic.Helpers.Security;
using System.ServiceModel.Activation;

namespace IISTestHost
{
    
    [ServiceContract]
//    [BusinessToBusiness("RawTcpClientCert1", "RawTcpServiceCert1")]
    //[PublicService]
        [SSLWithWindowsAuth]
    public interface IService1
    {
        [OperationContract]
        string GetData(int value);
    }

    [ServiceContract]
    [BusinessToBusiness("RawTcpClientCert1", "RawTcpServiceCert1")]
    //[PublicService]
    public interface IService1A
    {
        [OperationContract]
        string GetData(int value);
    }

}
