using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using DontPanic.Helpers;

namespace OverrideTests
{
    [ServiceContract]    
    public interface IMyService5
    {
        [OperationContract]
        string TestMe(string input);
    }

    class MyService5 : ServiceBase, IMyService5
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [TestClass]
    public class OverrideWithConfig
    {
        [TestMethod]
        public void OverrideWithConfig_1()
        {
            var factory = new ProxyFactory();
            Assert.AreEqual("hi", factory.Proxy<IMyService5>().TestMe("hi"));
        }

        [TestMethod]
        public void OverrideWithConfig_2()
        {
            var factory = new ProxyFactory();
            factory.Call<IMyService5>(proxy =>
                {
                    Assert.AreEqual("hi", proxy.TestMe("hi"));
                });
        }

        [TestMethod]
        public void OverrideWithConfig_3()
        {
            var factory = new ProxyFactory();
            factory.Call<IMyService5>(proxy =>
            {
                Assert.AreEqual("hi", proxy.TestMe("hi"));
            });
        }

    }
}
