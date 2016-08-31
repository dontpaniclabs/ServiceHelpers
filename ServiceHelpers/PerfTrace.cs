using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace DontPanic.Helpers
{
    /// <summary>
    /// This class just does a little tracing with performance of a call.
    /// </summary>
    class PerfTrace : IDisposable
    {
        Stopwatch _sw;
        Type _contract;
        string _methodName;

        public PerfTrace(Type contract, string methodName)
        {
            if (ServiceHelpersConfigSection.Settings != null && ServiceHelpersConfigSection.Settings.PerformanceTrace && contract != null && methodName != null)
            {
                _contract = contract;
                _methodName = methodName;

                _sw = new Stopwatch();
                _sw.Start();
            }
        }

        public void Dispose()
        {
            if (_sw != null)
            {
                _sw.Stop();
                Trace.WriteLine(string.Format("PerfTrace: Call time for {0}.{1} was {2}ms", _contract.FullName, _methodName, _sw.ElapsedMilliseconds));
            }
        }
    }
}
