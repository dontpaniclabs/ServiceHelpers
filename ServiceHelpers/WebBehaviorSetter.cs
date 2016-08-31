using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace DontPanic.Helpers
{
    static class WebBehaviorSetter
    {
        public static void AddWebBehavior(ServiceHostBase host)
        {
            if (ServiceHelpersConfigSection.Settings != null && ServiceHelpersConfigSection.Settings.MaskErrors == false)
            {
                if (host.Description.Endpoints != null)
                {                    
                    foreach (var e in host.Description.Endpoints)
                    {
                        bool add = true;

                        foreach (var b in e.Behaviors)
                        {
                            if (b is WebHttpBehavior)
                            {
                                add = false;
                            }
                        }

                        if (add)
                        {
                           
                        }
                    }                    
                }
            }
        }
    }
}
