using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using DontPanic.Helpers;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace BasicTests
{
    [DataContract]
    public class MyData
    {
        [DataMember]
        public int DataMemberInt { get; set; }

        [DataMember]
        public string DataMemberString { get; set; }
    }

    [DataContract]
    public struct MyStruct
    {
        [DataMember]
        public int DataMemberInt { get; set; }

        [DataMember]
        public string DataMemberString { get; set; }
    }

    [ServiceContract]
    [InProc("BasicTests.MyService2, BasicTests")]
    public interface IMyService2
    {
        [OperationContract]
        string TestMe(string input);
    }

    class MyService2 : ServiceBase, IMyService2
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    [InProc(typeof(MyService))]
    public interface IMyService
    {
        [OperationContract]
        string TestMe(string input);

        [OperationContract]
        void NoReturn();

        [OperationContract]
        long TryLong(long v);

        [OperationContract]
        void TryLong2(long v);

        [OperationContract]
        long TryLong3(string v);

        [OperationContract]
        MyData DataTest(MyData data);

        [OperationContract]
        MyStruct StructTest(MyStruct data);
    }

    class MyService : ServiceBase, IMyService
    {
        public string TestMe(string input)
        {
            return input;
        }

        public void NoReturn()
        {
            Console.WriteLine("no params, no return");
        }

        public void TryLong2(long v)
        {
            Console.WriteLine(v.ToString());
        }

        public long TryLong(long v)
        {
            return v;
        }

        public long TryLong3(string v)
        {
            return 10L;
        }

        public MyData DataTest(MyData data)
        {
            return data;
        }

        public MyStruct StructTest(MyStruct data)
        {
            return data;
        }
    }

    [ServiceContract]
    [InProc(typeof(SecurityHelper))]
    public interface ISecurityHelper
    {
        [OperationContract]
        Customer ValidateUser(long userId);
    }

    public class SecurityHelper : ISecurityHelper
    {
        public Customer ValidateUser(long userId)
        {
            Customer c = new Customer()
            {
                Name = "customer1",
                Id = 1,
                Code = "code",
            };
            return c;
        }
    }

    [DataContract]
    public class Customer
    {
        [DataMember, Required]
        public long Id { get; set; }
        [DataMember, Required]
        public string Name { get; set; }
        [DataMember, StringLength(50)]
        public string Code { get; set; }
    }


    [TestClass]
    public class BasicInProcTests
    {
        [TestMethod]
        public void BasicInProc_SecurityHelper()
        {
            ProxyFactory factory = new ProxyFactory();
            var proxy = factory.Proxy<ISecurityHelper>();
            proxy.ValidateUser(10);            
        }        

        [TestMethod]
        public void BasicInProc_String()
        {
            ProxyFactory factory = new ProxyFactory();
            factory.Call<IMyService2>(proxy =>
            {
                Assert.AreEqual("hi", proxy.TestMe("hi"));
            });
            Assert.AreEqual("hi", factory.Proxy<IMyService>().TestMe("hi"));
        }

        [TestMethod]
        public void BasicInProc_Demo()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IMyService>();
            var result = proxy.TestMe("hi");            
        }

        [TestMethod]
        public void BasicInProc_Simple()
        {
            ProxyFactory factory = new ProxyFactory();
            factory.Call<IMyService>(proxy =>
            {
                Assert.AreEqual("hi", proxy.TestMe("hi"));
            });
            Assert.AreEqual("hi", factory.Proxy<IMyService>().TestMe("hi"));
        }

        [TestMethod]
        public void BasicInProc_Proxy()
        {
            ProxyFactory factory = new ProxyFactory();
            Assert.AreEqual("hi", factory.Proxy<IMyService>().TestMe("hi"));
        }

        [TestMethod]
        public void BasicInProc_Proxy2()
        {
            ProxyFactory factory = new ProxyFactory();
            IMyService proxy = factory.Proxy<IMyService>();
            Assert.AreEqual("hi", proxy.TestMe("hi"));
        }

        [TestMethod]
        public void BasicInProc_Example()
        {
            ProxyFactory factory = new ProxyFactory();
            IMyService proxy = factory.Proxy<IMyService>();
            proxy.TestMe("hi");
        }

        [TestMethod]
        public void BasicInProc_Proxy_NoReturn()
        {
            ProxyFactory factory = new ProxyFactory();
            factory.Proxy<IMyService>().NoReturn();
        }

        [TestMethod]
        public void BasicInProc_Proxy_TryLong()
        {
            ProxyFactory factory = new ProxyFactory();
            Console.WriteLine(factory.Proxy<IMyService>().TryLong(4).ToString());
        }

        [TestMethod]
        public void BasicInProc_Proxy_TryLong2()
        {
            ProxyFactory factory = new ProxyFactory();
            factory.Proxy<IMyService>().TryLong2(4);
        }

        [TestMethod]
        public void BasicInProc_Proxy_TryLong3()
        {
            ProxyFactory factory = new ProxyFactory();
            factory.Proxy<IMyService>().TryLong3("hi");
        }

        [TestMethod]
        public void BasicInProc_Proxy_Data()
        {
            ProxyFactory factory = new ProxyFactory();
            var result = factory.Proxy<IMyService>().DataTest(new MyData() { DataMemberInt = 10, DataMemberString = "Hi" });
            Assert.AreEqual("Hi", result.DataMemberString);
            Assert.AreEqual(10, result.DataMemberInt);
        }

        [TestMethod]
        public void BasicInProc_Proxy_Struct()
        {
            ProxyFactory factory = new ProxyFactory();
            var result = factory.Proxy<IMyService>().StructTest(new MyStruct() { DataMemberInt = 10, DataMemberString = "Hi" });
            Assert.AreEqual("Hi", result.DataMemberString);
            Assert.AreEqual(10, result.DataMemberInt);
        }

        [TestMethod]
        public void BasicInProc_Proxy_Struct2()
        {
            ProxyFactory factory = new ProxyFactory();
            var proxy = factory.Proxy<IMyService>();
            var result = proxy.StructTest(new MyStruct() { DataMemberInt = 10, DataMemberString = "Hi" });
            Assert.AreEqual("Hi", result.DataMemberString);
            Assert.AreEqual(10, result.DataMemberInt);
        }

        [TestMethod]
        public void BasicInProc_ForceInProc()
        {
            ProxyFactory factory = new ProxyFactory();
            factory.Call<IMyService>(proxy =>
            {
                Assert.AreEqual("hi", proxy.TestMe("hi"));
            });
        }
    }
}
