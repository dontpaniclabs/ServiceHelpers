using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DontPanic.Helpers;

namespace BasicTests
{
    public interface IServiceBaseTest
    {
        string TestMe(string input);
    }

    public class ServiceBaseTest : ServiceBase, IServiceBaseTest
    {
        public string TestMe(string input)
        {
            var proxy = Factory.Proxy<IMyService>();
            return proxy.TestMe(input);
        }
    }



    /// <summary>
    /// Summary description for ServiceBaseTests
    /// </summary>
    [TestClass]
    public class ServiceBaseTests
    {       
        [TestMethod]
        public void ServiceBase_Factory()
        {
            var sb = new ServiceBaseTest();
            Assert.AreEqual("hi", sb.TestMe("hi"));
        }
    }
}
