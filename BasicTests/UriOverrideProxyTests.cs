using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.Reflection;
using DontPanic.Helpers;

namespace BasicTests
{
    [ServiceContract]
    public interface IUriOverrideProxy
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class UriOverrideProxy : IUriOverrideProxy
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [TestClass]
    public class UriOverrideProxyTests
    {
        [TestMethod]
        public void UriOverrideProxy_Basic()
        {
            string uri = "http://localhost/urioverride";
            using (var host = new ServiceHost(typeof(UriOverrideProxy), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IUriOverrideProxy), new WS2007HttpBinding(SecurityMode.None), uri);
                host.Open();

                var factory = new ProxyFactory();

                var proxy = factory.Proxy<IUriOverrideProxy>(uri);
                Assert.AreEqual("test", proxy.TestMe("test"));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TargetInvocationException))]
        public void UriOverrideProxy_Fail()
        {
            string uri = "http://localhost/urioverride";
            var factory = new ProxyFactory();

            var proxy = factory.Proxy<IUriOverrideProxy>(uri);
            Assert.AreEqual("test", proxy.TestMe("test"));
        }
    }
}
