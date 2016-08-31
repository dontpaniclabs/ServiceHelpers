using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using DontPanic.Helpers;

namespace IISTestHost
{    
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Service1 : ServiceBase, IService1
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

    }

}
