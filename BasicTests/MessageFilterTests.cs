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
    [InProc(typeof(MessageFilterService))]
    [ProxyMessageFilter(typeof(MessageFilterSample))]
    public interface IMessageFilterService
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class MessageFilterService : ServiceBase, IMessageFilterService
    {
        public string TestMe(string input)
        {
            Assert.AreEqual("hello", input);
            return input;
        }
    }

    public class MessageFilterSample : IMessageFilter
    {
        public bool PreFilter<I>(ProxyFactory factory, string method, object[] parameters, ref object result)
             where I : class
        {
            parameters[0] = "hello";
            return false;
        }

        public void PostFilter<I>(ProxyFactory factory, string method, object[] parameters, ref object result)
             where I : class
        {
            result = "bye";
        }
    }


    [ServiceContract]
    [InProc(typeof(MessageFilterService2))]
    [ProxyMessageFilter("BasicTests.MessageFilterSample, BasicTests")]
    public interface IMessageFilterService2
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class MessageFilterService2 : ServiceBase, IMessageFilterService2
    {
        public string TestMe(string input)
        {
            Assert.AreEqual("hello", input);
            return input;
        }
    }

    public class MessageFilterService2_2 : ServiceBase, IMessageFilterService2
    {
        public string TestMe(string input)
        {
            Assert.AreEqual("hello", input);
            return input;
        }
    }

    [TestClass]
    public class MessageFilterTests
    {
        [TestMethod]
        public void MessageFilter_ConfigType()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IMessageFilterService>();
            Assert.AreEqual("bye", proxy.TestMe("hi"));
        }

        [TestMethod]
        public void MessageFilter_ConfigString()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IMessageFilterService2>();
            Assert.AreEqual("bye", proxy.TestMe("hi"));
        }

        [TestMethod]
        public void MessageFilter_Override()
        {
            var factory = new ProxyFactory();
            factory.AddProxyOverride<IMessageFilterService2, MessageFilterService2_2>(new MessageFilterService2_2());
            var proxy = factory.Proxy<IMessageFilterService2>();
            Assert.AreEqual("bye", proxy.TestMe("hi"));
        }

    }
}
