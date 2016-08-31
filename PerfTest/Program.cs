using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using DontPanic.Helpers;
using System.ServiceModel;

namespace PerfTest
{
    [ServiceContract]
    //[InProc(typeof(MyService))]
    [InProc(typeof(MyService), 1)]
    public interface IMyService
    {
        [OperationContract]
        string TestMe(string input);
    }

    class MyService : ServiceBase, IMyService
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ProxyFactory();
            factory.LogEnabled = false;
            //factory.AddProxyOverride<IMyService, MyService>(new MyService());

            var sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            var proxy = factory.Proxy<IMyService>();
            for (int i = 0; i < 100000; i++)
            {
                proxy.TestMe("hi");
            }
            sw.Stop();
            Trace.WriteLine("Proxy time = " + sw.ElapsedMilliseconds);
            
        }
    }
}
