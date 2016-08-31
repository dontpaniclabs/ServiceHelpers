using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using DontPanic.Helpers;
using System.Reflection;

namespace BasicTests
{
    [ServiceContract]
    public interface IUriOverride
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class UriOverride : IUriOverride
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [TestClass]
    public class UriOverrideTests
    {
        [TestMethod]
        public void UriOverride_Basic()
        {
            string uri = "http://localhost/urioverride";
            using (var host = new ServiceHost(typeof(UriOverride), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IUriOverride), new WS2007HttpBinding(SecurityMode.None), uri);
                host.Open();

                var factory = new ProxyFactory();
                factory.AddEndpointAddressOverride<IUriOverride>(new Uri(uri));

                var proxy = factory.Proxy<IUriOverride>();
                Assert.AreEqual("test", proxy.TestMe("test"));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TargetInvocationException))]
        public void UriOverride_Fail()
        {
            string uri = "http://localhost/urioverride";
            var factory = new ProxyFactory();
            factory.AddEndpointAddressOverride<IUriOverride>(new Uri(uri));

            var proxy = factory.Proxy<IUriOverride>();
            Assert.AreEqual("test", proxy.TestMe("test"));
        }
    }
}
