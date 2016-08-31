using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using DontPanic.Helpers;

namespace InProcOnlyTests
{
    [ServiceContract]
    public interface ISimple
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class Simple : ISimple
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [TestClass]
    public class Basic
    {
        [TestMethod]
        public void Basic_Simple()
        {
            var factory = new ProxyFactory();
            var simple = factory.Proxy<ISimple>();
            Assert.AreEqual("hi", simple.TestMe("hi"));

            Assert.AreEqual("hi", simple.TestMe("hi"));

            Assert.AreEqual("hi", simple.TestMe("hi"));
        }
    }
}
