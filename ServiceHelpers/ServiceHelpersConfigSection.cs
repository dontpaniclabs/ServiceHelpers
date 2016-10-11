using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.ServiceModel;

namespace DontPanic.Helpers
{
    public class ServiceHelpersConfigSection : ConfigurationSection
    {
        public static ServiceHelpersConfigSection Settings =
            ConfigurationManager.GetSection("ServiceHelpers") as ServiceHelpersConfigSection;

        public ServiceHelpersConfigSection()
        {
        }

        [ConfigurationProperty("log", DefaultValue = false)]
        public bool Log
        {
            get { return (bool)this["log"]; }
            set { this["log"] = value; }
        }

        [ConfigurationProperty("enableInProc", DefaultValue = false)]
        public bool EnableInProc
        {
            get { return (bool)this["enableInProc"]; }
            set { this["enableInProc"] = value; }
        }

        [ConfigurationProperty("catchNonWcfFaults", DefaultValue = true)]
        public bool CatchNonWcfFaults
        {
            get { return (bool)this["catchNonWcfFaults"]; }
            set { this["catchNonWcfFaults"] = value; }
        }

        [ConfigurationProperty("performanceTrace", DefaultValue = false)]
        public bool PerformanceTrace
        {
            get { return (bool)this["performanceTrace"]; }
            set { this["performanceTrace"] = value; }
        }

        [ConfigurationProperty("maskErrors", DefaultValue = false)]
        public bool MaskErrors
        {
            get { return (bool)this["maskErrors"]; }
            set { this["maskErrors"] = value; }
        }

        [ConfigurationProperty("logger", IsRequired = false, DefaultValue = "")]
        public string Logger
        {
            get { return (string)this["logger"]; }
            set { this["logger"] = value; }
        }

        [ConfigurationProperty("inproc", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(ClientEndpointsCollection), AddItemName = "endpoint")]
        public ClientEndpointsCollection InProc
        {
            get { return (ClientEndpointsCollection)base["inproc"]; }
        }

        [ConfigurationProperty("external", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(ClientEndpointsCollection), AddItemName = "endpoint")]
        public ClientEndpointsCollection External
        {
            get { return (ClientEndpointsCollection)base["external"]; }
        }

        [ConfigurationProperty("sslport", DefaultValue = 0)]
        public int SSLPort
        {
            get { return (int)this["sslport"]; }
            set { this["sslport"] = value; }
        }

        public ClientEndpoint Endpoint(Type t)
        {
            ClientEndpoint result = null;

            if (InProc[t.Name] != null)
                result = InProc[t.Name];
            else if (InProc[t.FullName] != null)
                result = InProc[t.FullName];
            else if (External[t.Name] != null)
                result = External[t.Name];
            else if (External[t.FullName] != null)
                result = External[t.FullName];

            return result;
        }

    }

    public class ClientEndpointsCollection : ConfigurationElementCollection
    {
        new public ClientEndpoint this[string Name]
        {
            get
            {
                return (ClientEndpoint)BaseGet(Name);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ClientEndpoint();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ClientEndpoint)element).Contract;
        }
    }

    public class ClientEndpoint : ConfigurationElement
    {
        [ConfigurationProperty("contract", IsRequired = true)]
        public string Contract
        {
            get
            {
                return (string)this["contract"];
            }
            set
            {
                this["contract"] = value;
            }
        }

        [ConfigurationProperty("implementation", IsRequired = false)]
        public string Implementation
        {
            get
            {
                return (string)this["implementation"];
            }
            set
            {
                this["implementation"] = value;
            }
        }

        [ConfigurationProperty("wcf", IsRequired = false, DefaultValue = true)]
        public bool UseWcf
        {
            get
            {
                return (bool)this["wcf"];
            }
            set
            {
                this["wcf"] = value;
            }
        }

        [ConfigurationProperty("securityEnabled", IsRequired = false, DefaultValue = true)]
        public bool SecurityEnabled
        {
            get
            {
                return (bool)this["securityEnabled"];
            }
            set
            {
                this["securityEnabled"] = value;
            }
        }

        [ConfigurationProperty("address", IsRequired = false)]
        public string Address
        {
            get
            {
                return (string)this["address"];
            }
            set
            {
                this["address"] = value;
            }
        }

        [ConfigurationProperty("cacheCount", IsRequired = false)]
        public int CacheCount
        {
            get
            {
                return (int)this["cacheCount"];
            }
            set
            {
                this["cacheCount"] = value;
            }
        }

        [ConfigurationProperty("timeout", IsRequired = false, DefaultValue = 0)]
        public int Timeout
        {
            get
            {
                return (int)this["timeout"];
            }
            set
            {
                this["timeout"] = value;
            }
        }

        [ConfigurationProperty("channelFactory", IsRequired = false, DefaultValue = "")]
        public string ChannelFactory
        {
            get
            {
                return (string)this["channelFactory"];
            }
            set
            {
                this["channelFactory"] = value;
            }
        }
    }

    static class ConfigHelper
    {
        public static bool IsSecurityEnabled(ServiceHostBase serviceHostBase)
        {
            if (serviceHostBase != null && serviceHostBase.Description != null && serviceHostBase.Description.Endpoints != null)
            {
                foreach (var endpiont in serviceHostBase.Description.Endpoints)
                {
                    if (endpiont.Contract != null)
                    {
                        if (!IsSecurityEnabled(endpiont.Contract.ContractType))
                            return false;
                    }
                }
            }

            return true;
        }

        public static bool IsSecurityEnabled(Type contractType)
        {
            var result = true;
            var endpoint = FindClientEndpoint(contractType);

            if (endpoint != null)
            {
                result = endpoint.SecurityEnabled;
            }

            return result;
        }

        public static ClientEndpoint FindClientEndpoint(Type contractType)
        {
            ClientEndpoint result = null;

            if (ServiceHelpersConfigSection.Settings != null && ServiceHelpersConfigSection.Settings.InProc != null)
            {
                foreach (ClientEndpoint endpoint in ServiceHelpersConfigSection.Settings.InProc)
                {
                    if (endpoint.Contract == contractType.Name || endpoint.Contract == contractType.FullName)
                    {
                        result = endpoint;
                        break;
                    }
                }
            }

            return result;
        }
    }
}
