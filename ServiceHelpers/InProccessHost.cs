using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace DontPanic.Helpers
{
    class InProcessHost
    {
        private static Dictionary<string, string> _inProcessList = new Dictionary<string, string>();
        private static List<ServiceHost> _hosts = new List<ServiceHost>();
        private static object _lockObject = new object();

        /// <summary>
        /// Create an instance of the WCF service for local consumption.
        /// If service has already been created once it will just be returned.
        /// </summary>
        public static string CreateHost(Type serviceType, Type contractType)
        {
            string resultUrl = string.Empty;

            if(serviceType == null)
            {
                if (contractType != null)
                    throw new ArgumentNullException("serviceType", string.Format("Unable to create a service for {0}. ServiceType is null.", contractType.FullName));
                throw new ArgumentNullException("serviceType", "Unable to create a service for a null contractType. Code should never make it to here.");
            }                

            string key = serviceType.FullName;

            // Check to see if someone has already setup this host.
            if (_inProcessList.TryGetValue(key, out resultUrl))
                return resultUrl;

            lock (_lockObject)
            {
                // Check to see if someone created the host while we waited for lock.
                if (_inProcessList.TryGetValue(key, out resultUrl))
                    return resultUrl;

                // Create host
                ServiceHost host = new ServiceHost(serviceType);

                // Choose a unique URI
                resultUrl = "net.pipe://localhost/" + Guid.NewGuid().ToString();

                // Add endpoint
                host.AddServiceEndpoint(
                    contractType,
                    new NetNamedPipeBinding() {
                        MaxBufferPoolSize = int.MaxValue, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, 
                        ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas() 
                            { MaxArrayLength = int.MaxValue, MaxBytesPerRead = int.MaxValue, MaxDepth = int.MaxValue, MaxNameTableCharCount = int.MaxValue, MaxStringContentLength = int.MaxValue},
                    },
                    resultUrl);

                // Open the host
                host.Open();

                // Add to cached lists
                _hosts.Add(host);
                _inProcessList.Add(key, resultUrl);
            }

            return resultUrl;
        }
    }

}
