using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DontPanic.Helpers;
using System.ServiceModel;

namespace NonStandard
{
    [ServiceContract]
    [InProc(typeof(MyService))]
    public interface IMyService
    {
        [OperationContract]
        string TestMe(string input);
    }

    class MyService : ServiceBase, IMyService
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    /// <summary>
    /// Summary description for NoConfig
    /// </summary>
    [TestClass]
    public class NoConfig
    {     
        [TestMethod]
        public void NoConfig_InProc()
        {
            ProxyFactory factory = new ProxyFactory();
            factory.Call<IMyService>(proxy =>
            {
                Assert.AreEqual("hi", proxy.TestMe("hi"));
            });
            Assert.AreEqual("hi", factory.Proxy<IMyService>().TestMe("hi"));
        }
    }
}
