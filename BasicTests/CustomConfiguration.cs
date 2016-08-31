using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DontPanic.Helpers;
using System.ServiceModel;

namespace BasicTests
{
    [ServiceContract]
    public interface IConfigContract
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class ConfigContract : IConfigContract
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    public interface IConfigContractTcp
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class ConfigContractTcp : IConfigContractTcp
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    public interface IConfigContractHttp
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class ConfigContractHttp : IConfigContractHttp
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    public interface IConfigContractNoWcf
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class ConfigContractNoWcf : IConfigContractNoWcf
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    /// <summary>
    /// Summary description for CustomConfiguration
    /// </summary>
    [TestClass]
    public class CustomConfiguration
    {
        [TestMethod]
        public void CustomConfiguration_Load()
        {
            var enableInProc = ServiceHelpersConfigSection.Settings.EnableInProc;
            Assert.IsTrue(enableInProc);

            var endpoint = ServiceHelpersConfigSection.Settings.InProc["BasicTests.IConfigContract"];
            Assert.IsNotNull(endpoint);

            Assert.AreEqual("BasicTests.IConfigContract", endpoint.Contract);
            Assert.AreEqual("BasicTests.ConfigContract, BasicTests", endpoint.Implementation);
            Assert.AreEqual(true, endpoint.UseWcf);
        }

        [TestMethod]
        public void CustomConfiguration_NotLoaded()
        {
            var enableInProc = ServiceHelpersConfigSection.Settings.EnableInProc;
            Assert.IsTrue(enableInProc);

            var endpoint = ServiceHelpersConfigSection.Settings.InProc["Bad"];
            Assert.IsNull(endpoint);
        }

        [TestMethod]
        public void CustomConfiguration_CallInProc()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IConfigContract>();
            Assert.AreEqual("hi", proxy.TestMe("hi"));
        }

        [TestMethod]
        public void CustomConfiguration_CallTcp()
        {
            string uri = "net.tcp://localhost:10096/servicehelpersTCP";
            using (var host = new ServiceHost(typeof(ConfigContractTcp), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IConfigContractTcp), new NetTcpBinding(), uri);
                host.Open();

                var factory = new ProxyFactory();

                var proxy = factory.Proxy<IConfigContractTcp>();
                Assert.AreEqual("hi", proxy.TestMe("hi"));
            }
        }

        [TestMethod]
        public void CustomConfiguration_CallHttp()
        {
            string uri = "http://localhost/servicehelpers2";
            using (var host = new ServiceHost(typeof(ConfigContractHttp), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IConfigContractHttp), new WS2007HttpBinding(SecurityMode.None), uri);
                host.Open();

                var factory = new ProxyFactory();

                var proxy = factory.Proxy<IConfigContractHttp>();
                Assert.AreEqual("hi", proxy.TestMe("hi"));
            }
        }

        [TestMethod]
        public void CustomConfiguration_NoWcf()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IConfigContractNoWcf>();
            Assert.AreEqual("hi", proxy.TestMe("hi"));
        }

    }
}
