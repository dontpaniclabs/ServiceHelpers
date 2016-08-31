using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace DontPanic.Helpers
{
    public static class MaxSetter
    {
        const int tenMillion = 10000000;

        public static void SetMaxes(Binding inputBinding)
        {
            if (inputBinding is WS2007HttpBinding)
            {                
                var binding = inputBinding as WS2007HttpBinding;
                binding.MaxBufferPoolSize = tenMillion;
                binding.MaxReceivedMessageSize = tenMillion;
                binding.ReaderQuotas.MaxArrayLength = tenMillion;
                binding.ReaderQuotas.MaxBytesPerRead = tenMillion;
                binding.ReaderQuotas.MaxDepth = tenMillion;
                binding.ReaderQuotas.MaxStringContentLength = tenMillion;
                binding.ReaderQuotas.MaxNameTableCharCount = tenMillion;
            }
            if (inputBinding is NetTcpBinding)
            {
                var binding = inputBinding as NetTcpBinding;
                binding.ReaderQuotas.MaxArrayLength = tenMillion;
                binding.ReaderQuotas.MaxBytesPerRead = tenMillion;
                binding.ReaderQuotas.MaxDepth = tenMillion;
                binding.ReaderQuotas.MaxStringContentLength = tenMillion;
                binding.MaxReceivedMessageSize = tenMillion;
                binding.MaxBufferPoolSize = tenMillion;                
            }
        }

        public static void SetTimeouts<I>(Binding inputBinding)
        {
            var endpoint = ServiceHelpersConfigSection.Settings.Endpoint(typeof(I));
            if (endpoint != null && endpoint.Timeout > 0)
            {
                if (inputBinding is WS2007HttpBinding)
                {
                    var binding = inputBinding as WS2007HttpBinding;
                    binding.SendTimeout = new TimeSpan(0, 0, endpoint.Timeout);
                    binding.ReceiveTimeout = new TimeSpan(0, 0, endpoint.Timeout);
                }                
            }
        }
    }
}
