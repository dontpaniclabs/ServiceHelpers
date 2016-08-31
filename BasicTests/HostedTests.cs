using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using DontPanic.Helpers;

namespace BasicTests
{
    [ServiceContract]
    public interface IExternalService
    {
        [OperationContract]
        string TestMe(string input);
    }


    class ExternalService : ServiceBase, IExternalService
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    public interface IExternalServiceTcp
    {
        [OperationContract]
        string TestMe(string input);
    }


    class ExternalServiceTcp : ServiceBase, IExternalServiceTcp
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    /// <summary>
    /// Summary description for HostedTests
    /// </summary>
    [TestClass]
    public class HostedTests
    {
        public HostedTests()
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
        public void Hosted_HTTP()
        {
            string uri = "http://localhost/servicehelpers";
            using (var host = new ServiceHost(typeof(ExternalService), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IExternalService), new WS2007HttpBinding(), uri);
                host.Open();

                var factory = new ProxyFactory();
                factory.Call<IExternalService>(call =>
                    {
                        Assert.AreEqual("hi", call.TestMe("hi"));
                    });
            }
        }

        [TestMethod]
        public void Hosted_HTTP_Proxy()
        {
            string uri = "http://localhost/servicehelpers";
            using (var host = new ServiceHost(typeof(ExternalService), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IExternalService), new WS2007HttpBinding(), uri);
                host.Open();

                var factory = new ProxyFactory();

                // Generate a thin proxy.
                var proxy = factory.Proxy<IExternalService>();

                // Call the TestMe method. This will proxy the messsage thru the ProxyFactory.CallMethod.
                Assert.AreEqual("hi", proxy.TestMe("hi"));
            }
        }

        [TestMethod]
        public void Hosted_TCP()
        {
            string uri = "net.tcp://localhost:10095/servicehelpersTCP";
            using (var host = new ServiceHost(typeof(ExternalServiceTcp), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IExternalServiceTcp), new NetTcpBinding(), uri);
                host.Open();

                var factory = new ProxyFactory();
                factory.Call<IExternalServiceTcp>(call =>
                {
                    Assert.AreEqual("hi", call.TestMe("hi"));
                });
            }
        }

        [TestMethod]
        public void Hosted_Basic_NoFactory()
        {
            string uri = "net.tcp://localhost:10095/servicehelpersTCP";
            using (var host = new ServiceHost(typeof(ExternalServiceTcp), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IExternalServiceTcp), new NetTcpBinding(), uri);
                host.Open();

                var channelFactory = new ChannelFactory<IExternalServiceTcp>(
                    new NetTcpBinding(), new EndpointAddress(uri));
                var proxy = channelFactory.CreateChannel();
                Assert.AreEqual("hi", proxy.TestMe("hi"));
                channelFactory.Close();
            }
        }

        [TestMethod]
        public void Hosted_Basic_NoFactory_NoCaller()
        {
            string uri = "net.tcp://localhost:10095/servicehelpersTCP";
            using (var host = new ServiceHost(typeof(ExternalServiceTcp), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IExternalServiceTcp), new NetTcpBinding(), uri);
                host.Open();
            }
        }

        [TestMethod]
        public void Hosted_Basic_NoFactory_Better()
        {
            string uri = "net.tcp://localhost:10095/servicehelpersTCP";
            using (var host = new ServiceHost(typeof(ExternalServiceTcp), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IExternalServiceTcp), new NetTcpBinding(), uri);
                host.Open();

                var channelFactory = new ChannelFactory<IExternalServiceTcp>(
                    new NetTcpBinding(), new EndpointAddress(uri));
                var proxy = channelFactory.CreateChannel();

                try
                {
                    Assert.AreEqual("hi", proxy.TestMe("hi"));

                    try
                    {
                        channelFactory.Close();
                    }
                    catch
                    { }
                }
                catch
                {
                    if (channelFactory != null)
                    {
                        channelFactory.Abort();
                    }
                }
            }
        }
    }
}
