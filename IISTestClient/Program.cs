using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DontPanic.Helpers;
using IISTestHost;

namespace IISTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ProxyFactory factory = new ProxyFactory();
                var proxy = factory.Proxy<IService1>();

                for (int i = 0; i < 10; i++)
                    Console.WriteLine(proxy.GetData(10));


            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.ReadLine();
        }
    }
}
