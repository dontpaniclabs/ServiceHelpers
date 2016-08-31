using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using DontPanic.Helpers;
using System.Reflection;

namespace BasicTests
{
    [ServiceContract]
    [InProc(typeof(MyExceptions))]
    [ExceptionBehavior]
    public interface IMyExceptions
    {
        [OperationContract]
        string CommEx(string input);

        [OperationContract]
        string General(string input);
    }

    [ServiceContract]
    [InProc(typeof(MyExceptionsNoWcf), true)]
    [ExceptionBehavior]
    public interface IMyExceptionsNoWcf
    {
        [OperationContract]
        string CommEx(string input);

        [OperationContract]
        string General(string input);
    }

    [ServiceContract]
    [InProc(typeof(MyExceptionsMiddleTier))]
    [ExceptionBehavior]
    public interface IMyExceptionsMiddleIier
    {
        [OperationContract]
        string CommEx(string input);

        [OperationContract]
        string General(string input);
    }

    [ServiceContract]
    [InProc(typeof(MyExceptionsNoWcfMiddleTier), true)]
    [ExceptionBehavior]
    public interface IMyExceptionsNoWcfMiddleIier
    {
        [OperationContract]
        string CommEx(string input);

        [OperationContract]
        string General(string input);
    }

    class MyExceptions : ServiceBase, IMyExceptions
    {
        public string CommEx(string input)
        {
            throw new CommunicationException();
        }

        public string General(string input)
        {
            throw new Exception("test");
        }
    }

    class MyExceptionsNoWcf : ServiceBase, IMyExceptionsNoWcf
    {
        public string CommEx(string input)
        {
            throw new CommunicationException();
        }

        public string General(string input)
        {
            throw new Exception("test");
        }
    }

    class MyExceptionsMiddleTier : ServiceBase, IMyExceptionsMiddleIier
    {
        public string CommEx(string input)
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IMyExceptions>();
            return proxy.CommEx(input);
        }

        public string General(string input)
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IMyExceptions>();
            return proxy.General(input);
        }
    }

    class MyExceptionsNoWcfMiddleTier : ServiceBase, IMyExceptionsNoWcfMiddleIier
    {
        public string CommEx(string input)
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IMyExceptionsNoWcf>();
            return proxy.CommEx(input);
        }

        public string General(string input)
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IMyExceptionsNoWcf>();
            return proxy.General(input);
        }
    }
    
    [ServiceContract]
    [InProc(typeof(MyExceptionsNoBase))]
    public interface IMyExceptionsNoBase
    {
        [OperationContract]
        string CommEx(string input);

        [OperationContract]
        string General(string input);

    }

    class MyExceptionsNoBase : IMyExceptionsNoBase
    {
        public string CommEx(string input)
        {
            throw new CommunicationException();
        }

        public string General(string input)
        {
            throw new Exception("test");
        }
    }

    [TestClass]
    public class ExceptionTests
    {
        [TestMethod]
        [ExpectedException(typeof(FaultException<ServiceError>))]
        public void Exception_General_NoWcf()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IMyExceptionsNoWcf>();
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

        [TestMethod]
        [ExpectedException(typeof(FaultException<ServiceError>))]
        public void Exception_TwoTier_Wcf()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IMyExceptionsMiddleIier>();
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

        [TestMethod]
        [ExpectedException(typeof(FaultException<ServiceError>))]
        public void Exception_TwoTier_NoWcf()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IMyExceptionsNoWcfMiddleIier>();
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

        [TestMethod]
        [ExpectedException(typeof(TargetInvocationException))]
        public void Exception_Comm_NoBase()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IMyExceptionsNoBase>();
            proxy.CommEx(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(TargetInvocationException))]
        public void Exception_General_NoBase()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IMyExceptionsNoBase>();
            proxy.General(string.Empty);
        }

    }

}
