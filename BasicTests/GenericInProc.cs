using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using DontPanic.Helpers;

namespace BasicTests
{
    [ServiceContract(Namespace = "http://RedV.EliteForm/2011/07/12")]
    public interface IBaseGenericContract<T>
    {
        [OperationContract]
        T Find(int id);
    }

    [ServiceContract(Namespace = "http://RedV.EliteForm/2011/07/12")]
    [InProc(typeof(GenericService))]
    public interface IGenericContract : IBaseGenericContract<int>
    {

    }

    public class GenericService : IGenericContract
    {
        public int Find(int id)
        {
            return id;
        }
    }

    [ServiceContract(Namespace = "http://RedV.EliteForm/2011/07/12")]
    public interface IBaseBaseGenericContract2Deep<T>
    {
        [OperationContract]
        T FindOther(int id);
    }

    [ServiceContract(Namespace = "http://RedV.EliteForm/2011/07/12")]
    public interface IBaseGenericContract2Deep<T> : IBaseBaseGenericContract2Deep<T>
    {
        [OperationContract]
        T Find(int id);
    }

    [ServiceContract(Namespace = "http://RedV.EliteForm/2011/07/12")]
    [InProc(typeof(GenericService2Deep))]
    public interface IGenericContract2Deep : IBaseGenericContract2Deep<int>
    {

    }

    public class GenericService2Deep : IGenericContract2Deep
    {
        public int Find(int id)
        {
            return id;
        }

        public int FindOther(int id)
        {
            return id;
        }
    }

    /// <summary>
    /// Summary description for GenericInProc
    /// </summary>
    [TestClass]
    public class GenericInProc
    {

        [TestMethod]
        public void GenericInProc_Raw()
        {
            string uri = "net.tcp://localhost:10095/genericContractTCP";
            using (var host = new ServiceHost(typeof(GenericService), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IGenericContract), new NetTcpBinding(), uri);
                host.Open();

                var factory = new ProxyFactory();
                factory.Call<IGenericContract>(call =>
                {
                    Assert.AreEqual(10, call.Find(10));
                });
            }

        }

        [TestMethod]
        public void GenericInProc_Factory()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IGenericContract>();
            Assert.AreEqual(10, proxy.Find(10));
        }

        [TestMethod]
        public void GenericInProc_Factory_2Deep()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IGenericContract2Deep>();
            Assert.AreEqual(10, proxy.Find(10));
            Assert.AreEqual(10, proxy.FindOther(10));
        }

    }
}
