using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DontPanic.Helpers;
using BasicTests;
using System.ServiceModel;

namespace OldDefaults
{
    [TestClass]
    public class OldDefaultTests
    {
        [TestMethod]
        [ExpectedException(typeof(FaultException<ServiceError>))]
        public void Exception_Comm()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IMyExceptions>();
            proxy.CommEx(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ServiceError>))]
        public void NestedContract_Test2()
        {
            var var = new ProxyFactory();
            var proxy = var.Proxy<IMyNested>();

            try
            {
                proxy.Test2();
            }
            catch (FaultException<ServiceError> faultEx)
            {
                Assert.AreEqual(faultEx.Detail.ExceptionType, "System.Exception");
                Assert.AreEqual(faultEx.Detail.ExceptionMessage, "TEST2");
                Console.WriteLine("ExceptionDetail: " + faultEx.Detail.ExceptionDetail);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FaultException<ServiceError>))]
        public void NestedContract_Test1()
        {
            var var = new ProxyFactory();
            var proxy = var.Proxy<IMyNested>();

            try
            {
                proxy.Test1();
            }
            catch (FaultException<ServiceError> faultEx)
            {
                Assert.AreEqual(faultEx.Detail.ExceptionType, "System.Exception");
                Assert.AreEqual(faultEx.Detail.ExceptionMessage, "TEST1");
                Console.WriteLine("ExceptionDetail: " + faultEx.Detail.ExceptionDetail);
                throw;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(FaultException<ServiceError>))]
        public void Exception_General()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IMyExceptions>();
            try
            {
                proxy.General(string.Empty);
            }
            catch (FaultException<ServiceError> faultEx)
            {
                Assert.AreEqual(faultEx.Detail.ExceptionType, "System.Exception");
                Assert.AreEqual(faultEx.Detail.ExceptionMessage, "test");
                Console.WriteLine("ExceptionDetail: " + faultEx.Detail.ExceptionDetail);
                throw;
            }
        }

    }
}
