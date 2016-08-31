using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using DontPanic.Helpers.Security;

namespace CloudSSLHost
{    
    [ServiceContract]
    [SSLOnly]
    public interface IService1
    {
        [OperationContract]
        string GetData(int value);
    }
}
