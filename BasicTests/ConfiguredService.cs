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
    public interface IMyConfiguredService
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class MyConfiguredService : ServiceBase, IMyConfiguredService
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    [InProc(typeof(ConfigOld))]
    public interface IConfigOld
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class ConfigOld : ServiceBase, IConfigOld
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    public class ConfigOldFake : ServiceBase, IConfigOld
    {
        public string TestMe(string input)
        {
            return "bye";
        }
    }

    [TestClass]
    public class ConfiguredService
    {
        [TestMethod]
        public void ConfiguredService_Basic()
        {
            string uri = "http://localhost/servicehelpers3";
            using(var host = new ServiceHost(typeof(MyConfiguredService), new Uri(uri)))
            {
                host.Open();
            }
        }

        [TestMethod]
        public void ConfiguredService_Old()
        {
            ProxyFactory factory = new ProxyFactory();
            var proxy = factory.Proxy<IConfigOld>();
            Assert.AreEqual("bye", proxy.TestMe("hi"));
        }

    }
}
