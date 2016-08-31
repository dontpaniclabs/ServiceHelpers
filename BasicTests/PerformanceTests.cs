using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DontPanic.Helpers;
using System.Diagnostics;
using System.ServiceModel;

namespace BasicTests
{
    /// <summary>
    /// Summary description for PerformanceTests
    /// </summary>
    [TestClass]
    public class PerformanceTests
    {
        public PerformanceTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Performance_InProc()
        {
            var factory = new ProxyFactory();
            factory.LogEnabled = false;
            
            var sw = new Stopwatch();

            MyService service2 = new MyService();
            service2.TestMe("Hi");

            sw.Reset();
            sw.Start();
            for (int i = 0; i < 10000; i++)
            {
                MyService service = new MyService();
                Assert.AreEqual("hi", service.TestMe("hi"));                
            }
            sw.Stop();
            Trace.WriteLine("Native time = " + sw.ElapsedMilliseconds);

            factory.Call<IMyService>(proxy =>
            {
                proxy.TestMe("hi");
            });

            sw.Reset();
            sw.Start();
            for (int i = 0; i < 10000; i++)
            {
                factory.Call<IMyService>(proxy =>
                {
                    Assert.AreEqual("hi", proxy.TestMe("hi"));
                });
            }
            sw.Stop();
            Trace.WriteLine("Lambda time = " + sw.ElapsedMilliseconds);

            factory.Proxy<IMyService>().TestMe("hi");

            sw.Reset();
            sw.Start();
            for (int i = 0; i < 10000; i++)
            {
                Assert.AreEqual("hi", factory.Proxy<IMyService>().TestMe("hi"));
            }
            sw.Stop();
            Trace.WriteLine("Proxy time = " + sw.ElapsedMilliseconds);

            factory.Proxy<IProxyCached>().TestMe("hi");

            sw.Reset();
            sw.Start();
            for (int i = 0; i < 10000; i++)
            {
                Assert.AreEqual("hi", factory.Proxy<IProxyCached>().TestMe("hi"));
            }
            sw.Stop();
            Trace.WriteLine("Proxy Cached time = " + sw.ElapsedMilliseconds);
        }

        //[TestMethod]
        public void Performance_NativeThruProxy()
        {            
            var factory = new ProxyFactory();
            factory.LogEnabled = false;
            factory.AddProxyOverride<IMyService, MyService>(new MyService());

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            for (int i = 0; i < 10000; i++)
            {
                Assert.AreEqual("hi", factory.Proxy<IMyService>().TestMe("hi"));
            }
            sw.Stop();
            Trace.WriteLine("Native thru Proxy time = " + sw.ElapsedMilliseconds);
        }

        //[TestMethod]
        public void Performance_CachedProxy()
        {
            var factory = new ProxyFactory();
            factory.LogEnabled = false;

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            for (int i = 0; i < 10000; i++)
            {
                Assert.AreEqual("hi", factory.Proxy<IProxyCached>().TestMe("hi"));
            }
            sw.Stop();
            Trace.WriteLine("Proxy cached time = " + sw.ElapsedMilliseconds);
        }

        //[TestMethod]
        public void Performance_Proxy()
        {
            var factory = new ProxyFactory();
            factory.LogEnabled = false;            

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            for (int i = 0; i < 10000; i++)
            {
                Assert.AreEqual("hi", factory.Proxy<IMyService>().TestMe("hi"));
            }
            sw.Stop();
            Trace.WriteLine("Proxy time = " + sw.ElapsedMilliseconds);
        }

        //[TestMethod]
        public void Performance_NoWcf()
        {
            var factory = new ProxyFactory();
            factory.LogEnabled = false;

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            for (int i = 0; i < 10000; i++)
            {
                Assert.AreEqual("hi", factory.Proxy<INoWcfService>().TestMe("hi"));
            }
            sw.Stop();
            Trace.WriteLine("No Wcf Proxy time = " + sw.ElapsedMilliseconds);
        }

        //[TestMethod]
        public void Performance_NoWcf2()
        {
            var factory = new ProxyFactory();
            factory.LogEnabled = false;
            var proxy = factory.Proxy<INoWcfService>();

            Stopwatch sw = new Stopwatch();

            sw.Reset();
            sw.Start();
            for (int i = 0; i < 10000; i++)
            {
                Assert.AreEqual("hi", proxy.TestMe("hi"));
            }
            sw.Stop();
            Trace.WriteLine("No Wcf Proxy time 2 = " + sw.ElapsedMilliseconds);
        }

        //[TestMethod]
        public void Performance_TCP()
        {
            string uri = "net.tcp://localhost:10095/servicehelpersTCP";
            using (var host = new ServiceHost(typeof(ExternalServiceTcp), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IExternalServiceTcp), new NetTcpBinding(), uri);
                host.Open();

                var sw = new Stopwatch();
                sw.Reset();
                sw.Start();

                var factory = new ProxyFactory();
                factory.LogEnabled = false;

                var proxy = factory.Proxy<IExternalServiceTcp>();
                for (int i = 0; i < 10000; i++)
                {
                    Assert.AreEqual("hi", factory.Proxy<IExternalServiceTcp>().TestMe("hi"));
                }
                
                sw.Stop();

                Trace.WriteLine("Tcp time = " + sw.ElapsedMilliseconds);
            }
        }

        //[TestMethod]
        public void Performance_TCP_NoFactory()
        {
            string uri = "net.tcp://localhost:10095/servicehelpersTCP";
            using (var host = new ServiceHost(typeof(ExternalServiceTcp), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IExternalServiceTcp), new NetTcpBinding(), uri);
                host.Open();

                var sw = new Stopwatch();
                sw.Reset();
                sw.Start();

                var channelFactory = new ChannelFactory<IExternalServiceTcp>(
                    new NetTcpBinding(), new EndpointAddress(uri));
                var proxy = channelFactory.CreateChannel();
                               
                for (int i = 0; i < 10000; i++)
                {
                    Assert.AreEqual("hi", proxy.TestMe("hi"));
                }
                channelFactory.Close();
                sw.Stop();

                Trace.WriteLine("Tcp time = " + sw.ElapsedMilliseconds);
            }



        }

    }
}
