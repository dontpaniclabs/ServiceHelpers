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
    [InProc(typeof(NoWcfService), true)]
    public interface INoWcfService
    {
        string TestMe(string input);
    }

    class NoWcfService : INoWcfService
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [TestClass]
    public class NoWcf
    {       
        [TestMethod]
        public void NoWcf_Test()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<INoWcfService>();
            Assert.AreEqual("hi", proxy.TestMe("hi"));
        }
    }
}
