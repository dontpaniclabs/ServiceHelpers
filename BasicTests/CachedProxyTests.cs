using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DontPanic.Helpers;
using System.ServiceModel;
using System.Threading;

namespace BasicTests
{
    [ServiceContract]
    [InProc(typeof(ProxyCached), 1)]
    public interface IProxyCached
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class ProxyCached : IProxyCached
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    [InProc(typeof(ProxyCached2), 1)]
    public interface IProxyCached2
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class ProxyCached2 : IProxyCached2
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    public interface IProxyCached3
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class ProxyCached3 : IProxyCached3
    {
        public string TestMe(string input)
        {
            return input;
        }
    }


    /// <summary>
    /// Summary description for CachedProxyTests
    /// </summary>
    [TestClass]
    public class CachedProxyTests
    {
        [TestMethod]
        public void CachedProxy_2Calls()
        {
            var factory = new ProxyFactory();
            factory.LogEnabled = false;

            Assert.AreEqual("hi", factory.Proxy<IProxyCached>().TestMe("hi"));
            Assert.AreEqual("hi", factory.Proxy<IProxyCached>().TestMe("hi"));
        }

        [TestMethod]
        public void CachedProxy_2Calls_Config()
        {
            var factory = new ProxyFactory();
            factory.LogEnabled = false;

            Assert.AreEqual("hi", factory.Proxy<IProxyCached2>().TestMe("hi"));
            Assert.AreEqual("hi", factory.Proxy<IProxyCached2>().TestMe("hi"));
        }

        [TestMethod]
        public void CachedProxy_KillHost()
        {
            var factory = new ProxyFactory();
            factory.LogEnabled = false;

            string uri = "http://localhost/servicehelpers";
            using (var host = new ServiceHost(typeof(ProxyCached3), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IProxyCached3), new WS2007HttpBinding(SecurityMode.None), uri);
                host.Open();

                Assert.AreEqual("hi", factory.Proxy<IProxyCached3>().TestMe("hi"));
            }

            Thread.Sleep(10);

            using (var host = new ServiceHost(typeof(ProxyCached3), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IProxyCached3), new WS2007HttpBinding(SecurityMode.None), uri);
                host.Open();

                Assert.AreEqual("hi", factory.Proxy<IProxyCached3>().TestMe("hi"));
            }
        }
    }
}
