using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using DontPanic.Helpers;

namespace SecurityTests
{
    [ServiceContract]
    [PublicService()]
    public interface IPublicService1
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class PublicService1 : ServiceBase, IPublicService1
    {
        public string TestMe(string input)
        {
            return input;
        }
    }


    /// <summary>
    /// Summary description for PublicServiceTests
    /// </summary>
    [TestClass]
    public class PublicServiceTests
    {
        [TestMethod]
        public void PublicService_OpenOnly()
        {
            string uri = "http://localhost/servicehelpers/public";
            using (var host = new ServiceHost(typeof(PublicService1), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IPublicService1), new WS2007HttpBinding(), uri);
                host.Open();
            }
        }

        [TestMethod]
        public void PublicService_TestCall()
        {
            string uri = "http://localhost:8080/servicehelpers/public";
            using (var host = new ServiceHost(typeof(PublicService1), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IPublicService1), new WS2007HttpBinding(), uri);
                host.Open();

                ProxyFactory factory = new ProxyFactory();
                var proxy = factory.Proxy<IPublicService1>();
                Assert.AreEqual("hi", proxy.TestMe("hi"));               
            }
        }
    }
}
