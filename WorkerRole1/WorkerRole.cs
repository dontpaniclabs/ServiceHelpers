using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using System.ServiceModel;
using DontPanic.Helpers;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        List<ServiceHost> _hosts = new List<ServiceHost>();

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("WorkerRole1 entry point called", "Information");

            while (true)
            {
                Thread.Sleep(10000);
                Trace.WriteLine("Working", "Information");
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.            

            StartServices();

            return base.OnStart();
        }

        public override void OnStop()
        {
            StopServices();

            base.OnStop();
        }

        private void StartServices()
        {
            IPEndPoint ip = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Services"].IPEndpoint;
            string uri = string.Format("http://{0}:{1}/MyService", ip.Address, ip.Port);
            StartService(uri, typeof(MyService), typeof(IMyService));
        }

        private void StopServices()
        {
            foreach (var host in _hosts)
            {
                try
                {
                    host.Close();
                }
                catch (Exception ex)
                {
                    Log("Exception while stopping service", ex);
                }
            }
        }

        private void StartService(string uri, Type service, Type contract)
        {
            try
            {
                var host = new ServiceHost(service, new Uri(uri));

                host.AddServiceEndpoint(contract, new WS2007HttpBinding(), uri);
                host.Open();
                
                _hosts.Add(host);
            }
            catch (Exception ex)
            {
                Log(string.Format("Exception while starting service {0} {1} {2}", uri, service.ToString(), contract.ToString()), ex);
            }
        }

        private void Log(string message, Exception ex)
        {
            var logger = new EnterpriseLogger();
            logger.LogException(message, ex);
        }
    }
}
