using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace OpenPort
{
    class Program
    {
        static void Main(string[] args)
        {
            Process.Start("netsh", string.Format("http add urlacl url=http://+:{0}/MyService user=everyone listen=yes delegate=yes",
                RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Services"].IPEndpoint.Port.ToString())); 
        }
    }
}
