using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DontPanic.Helpers;
using WorkerRole1;

namespace WorkerRoleTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var proxy = new ProxyFactory().Proxy<IMyService>();
            Console.WriteLine(proxy.TestMe("hi"));

            Console.ReadLine();
        }
    }
}
