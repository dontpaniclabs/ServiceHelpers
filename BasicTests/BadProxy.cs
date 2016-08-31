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
    public interface IBadService
    {
        [OperationContract]
        void TestMe();

        // property bad
        string Foo {get;}
    }

    public class BadService : IBadService
    {
        public void TestMe()
        {            
        }

        public string Foo
        {
            get
            {
                return "test;";
            }
        }
    }

    
    [TestClass]
    public class BadProxy
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BadProxy_CreateProxyFails()
        {
            ProxyFactory factory = new ProxyFactory();
            var proxy = factory.Proxy<IBadService>();
        }
    }
}
