using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using DontPanic.Helpers;

namespace OverrideTests
{
    [ServiceContract]
    [InProc(typeof(MyService), true)]
    public interface IMyService
    {
        [OperationContract]
        string TestMe(string input);

        [OperationContract]
        string Nested();
    }


    class MyService : ServiceBase, IMyService
    {
        public string TestMe(string input)
        {
            return input;
        }

        public string Nested()
        {
            return Factory.Proxy<IMyServiceNested>().TestMe();
        }
    }

    class MyService2 : ServiceBase, IMyService
    {
        public string TestMe(string input)
        {
            return "hello";
        }


        public string Nested()
        {
            return Factory.Proxy<IMyServiceNested>().TestMe();
        }
    }

    [ServiceContract]
    [InProc(typeof(MyNestedService1), true)]
    public interface IMyServiceNested
    {
        [OperationContract]
        string TestMe();
    }

    class MyNestedService1 : ServiceBase, IMyServiceNested
    {
        public string TestMe()
        {
            return "hello";
        }
    }

    class MyNestedService2 : ServiceBase, IMyServiceNested
    {
        public string TestMe()
        {
            return "goodbye";
        }
    }

    [TestClass]
    public class Overrides
    {
        [TestMethod]
        public void Overrides_None()
        {
            ProxyFactory factory = new ProxyFactory();
            factory.Call<IMyService>(proxy =>
                {
                    Assert.AreEqual("hi", proxy.TestMe("hi"));
                });
        }

        [TestMethod]
        public void Overrides_Override()
        {
            ProxyFactory factory = new ProxyFactory();
            MyService2 service2 = new MyService2();
            factory.AddProxyOverride<IMyService, MyService2>(service2);

            factory.Call<IMyService>(proxy =>
            {
                Assert.AreEqual("hello", proxy.TestMe("hi"));
            });
        }

        [TestMethod]
        public void Overrides_OverrideNew()
        {
            ProxyFactory factory = new ProxyFactory();
            MyService2 service2 = new MyService2();
            factory.AddProxyOverride<IMyService>(service2);

            factory.Call<IMyService>(proxy =>
            {
                Assert.AreEqual("hello", proxy.TestMe("hi"));
            });
        }

        [TestMethod]
        public void Overrides_Proxy_OverrideNew()
        {
            ProxyFactory factory = new ProxyFactory();
            MyService2 service2 = new MyService2();
            factory.AddProxyOverride<IMyService>(service2);

            var proxy = factory.Proxy<IMyService>();
            Assert.AreEqual("hello", proxy.TestMe("hi"));
        }

        [TestMethod]
        public void Overrides_Proxy_Override_FixFactory()
        {
            ProxyFactory factory = new ProxyFactory();
            MyService2 service2 = new MyService2() { Factory = factory };
            factory.AddProxyOverride<IMyService>(service2);

            var proxy = factory.Proxy<IMyService>();
            Assert.AreEqual("hello", proxy.TestMe("hi"));
        }

        [TestMethod]
        public void Overrides_Proxy_NoOverride()
        {
            ProxyFactory factory = new ProxyFactory();

            var proxy = factory.Proxy<IMyService>();            
            Assert.AreEqual("hi", proxy.TestMe("hi"));
        }

        [TestMethod]
        public void Overrides_Proxy_Override()
        {
            ProxyFactory factory = new ProxyFactory();
            MyService2 service2 = new MyService2();
            factory.AddProxyOverride<IMyService, MyService2>(service2);

            var proxy = factory.Proxy<IMyService>();
            Assert.AreEqual("hello", proxy.TestMe("hi"));
        }

        [TestMethod]
        public void Overrides_Proxy_Override_Twice()
        {
            ProxyFactory factory = new ProxyFactory();
            MyService2 service2 = new MyService2();
            factory.AddProxyOverride<IMyService>(service2);
            factory.AddProxyOverride<IMyService>(service2);

            var proxy = factory.Proxy<IMyService>();
            Assert.AreEqual("hello", proxy.TestMe("hi"));
        }

        [TestMethod]
        public void Overrides_Proxy_OverrideNested_None()
        {
            ProxyFactory factory = new ProxyFactory();            

            var proxy = factory.Proxy<IMyService>();
            Assert.AreEqual("hello", proxy.Nested());
        }

        [TestMethod]
        public void Overrides_Proxy_OverrideNested_Override()
        {
            ProxyFactory factory = new ProxyFactory();

            var proxy = factory.Proxy<IMyService>();
            var overrideService = new MyNestedService2();

            factory.AddProxyOverride<IMyServiceNested>(overrideService);

            Assert.AreEqual("goodbye", proxy.Nested());
        }
    }
}
