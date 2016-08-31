using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using DontPanic.Helpers;
using System.Threading;

namespace BasicTests
{
    [ServiceContract]
    public interface ITimeoutTest
    {
        [OperationContract]
        string TestMe();
    }

    
    public class TimeoutTest : ITimeoutTest
    {        
        public string TestMe()
        {
            Thread.Sleep(90 * 1000); // sleep 90 seconds
            return "hi";
        }
    }

    [TestClass]
    public class Timeout
    {
        [TestMethod]
        public void TimeoutTest()
        {
            string uri = "http://localhost:9595/timeouttest";
            using (var host = new ServiceHost(typeof(TimeoutTest), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(ITimeoutTest), new WS2007HttpBinding(SecurityMode.None), uri);
                host.Open();

                var factory = new ProxyFactory();
                
                var proxy = factory.Proxy<ITimeoutTest>();
                
                Assert.AreEqual("hi", proxy.TestMe());
            }
        }
    }
}
