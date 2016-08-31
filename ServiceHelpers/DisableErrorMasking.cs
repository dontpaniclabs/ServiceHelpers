using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace DontPanic.Helpers
{
    public static class DisableErrorMasking
    {
        public static void Disable(ServiceHostBase host)
        {
            if (ServiceHelpersConfigSection.Settings != null && ServiceHelpersConfigSection.Settings.MaskErrors == false)
            {
                if (host.Description.Behaviors != null)
                {
                    bool add = true;

                    foreach (var b in host.Description.Behaviors)
                    {
                        if (b is ServiceDebugBehavior)
                        {
                            (b as ServiceDebugBehavior).IncludeExceptionDetailInFaults = true;
                            add = false;
                        }
                    }

                    if (add)
                    {
                        host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
                    }
                }
            }
        }
    }
}
