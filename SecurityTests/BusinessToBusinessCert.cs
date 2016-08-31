using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using DontPanic.Helpers;
using System.ServiceModel.Security;
using System.Security.Cryptography.X509Certificates;

namespace SecurityTests
{
    [ServiceContract]
    [BusinessToBusiness("RawTcpClientCert1", "RawTcpServiceCert1")]
    public interface IBusinessToBusiness1
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class BusinessToBusiness1 : ServiceBase, IBusinessToBusiness1
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    [BusinessToBusiness("B2BCurrentUserClient", "B2BCurrentUserService")]
    public interface IBusinessToBusinessCurrentUser
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class BusinessToBusinessCurrentUser : ServiceBase, IBusinessToBusinessCurrentUser
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    [BusinessToBusiness("B2BLocalMachineClient", "B2BLocalMachineService")]
    public interface IBusinessToBusinessLocalMachine
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class BusinessToBusinessLocalMachine : ServiceBase, IBusinessToBusinessLocalMachine
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    [BusinessToBusiness("B2BLocalMachineClient", "B2BLocalMachineService")]
    public interface IBusinessToBusinessNoErrorMasking
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class BusinessToBusinessNoErrorMasking : ServiceBase, IBusinessToBusinessNoErrorMasking
    {
        public string TestMe(string input)
        {
            throw new Exception("hi");
        }
    }

    [ServiceContract]
    [BusinessToBusiness("B2BCurrentUserClient", "B2BCurrentUserService")]
    public interface IBusinessToBusinessCached
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class BusinessToBusinessCached : ServiceBase, IBusinessToBusinessCached
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    [BusinessToBusiness("B2BCurrentUserClient", "B2BCurrentUserService")]
    [InProc(typeof(BusinessToBusinessPipe))]
    public interface IBusinessToBusinessPipe
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class BusinessToBusinessPipe : ServiceBase, IBusinessToBusinessPipe
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    [BusinessToBusiness("B2BCurrentUserClient", "B2BCurrentUserService")]
    public interface IBusinessToBusinessTcp
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class BusinessToBusinessTcp : ServiceBase, IBusinessToBusinessTcp
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    [BusinessToBusiness("B2BCurrentUserClient", "B2BCurrentUserService")]
    public interface IBusinessToBusinessInProc
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class BusinessToBusinessInProc : ServiceBase, IBusinessToBusinessInProc
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [TestClass]
    public class BusinessToBusinessCertTests
    {
        [TestInitialize()]
        public void MyTestInitialize() 
        {
            ConfigureTestCerts.Configure();
        }


        [TestCleanup()]
        public void MyTestCleanup() 
        {
            ConfigureTestCerts.Configure();
        }

        [TestMethod]
        public void BusinessToBusinessCert_UseAttribute_OpenOnly()
        {
            string uri = "http://localhost/servicehelpers";
            using (var host = new ServiceHost(typeof(BusinessToBusiness1), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IBusinessToBusiness1), new WS2007HttpBinding(), uri);
                host.Open();
            }
        }

        [TestMethod]
        public void BusinessToBusinessCert_UseAttribute_LocalMachineCert()
        {
            string uri = "http://localhost/servicehelpers";
            using (var host = new ServiceHost(typeof(BusinessToBusiness1), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IBusinessToBusiness1), new WS2007HttpBinding(), uri);
                host.Open();

                var clientBinding = new WS2007HttpBinding(SecurityMode.Message);
                clientBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                clientBinding.Security.Message.EstablishSecurityContext = false;
                clientBinding.Security.Message.NegotiateServiceCredential = false;

                EndpointIdentity identity = new DnsEndpointIdentity("RawTcpServiceCert1");

                var factory = new ChannelFactory<IBusinessToBusiness1>(clientBinding, new EndpointAddress(new Uri(uri), identity));
                factory.Credentials.ClientCertificate.SetCertificate(
                    "CN=RawTcpClientCert1",
                    System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    System.Security.Cryptography.X509Certificates.StoreName.My);
                factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                var serviceCert = "RawTcpServiceCert1";

                StoreLocation serviceLocation = StoreLocation.LocalMachine;
                CertHelper.TryGetCertLocation(serviceCert, out serviceLocation, true);

                factory.Credentials.ServiceCertificate.SetDefaultCertificate(
                    serviceLocation,
                    System.Security.Cryptography.X509Certificates.StoreName.My,
                    X509FindType.FindBySubjectName,
                    serviceCert);

                factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                var channel = factory.CreateChannel();
                Assert.AreEqual("hi", channel.TestMe("hi"));

                factory.Close();
            }
        }

        [TestMethod]
        public void BusinessToBusinessCert_UseAttribute_CurrentUserCert()
        {
            string uri = "http://localhost/servicehelpers/b2b";
            using (var host = new ServiceHost(typeof(BusinessToBusinessCurrentUser), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IBusinessToBusinessCurrentUser), new WS2007HttpBinding(), uri);
                host.Open();

                var clientBinding = new WS2007HttpBinding(SecurityMode.Message);
                clientBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                clientBinding.Security.Message.EstablishSecurityContext = false;
                clientBinding.Security.Message.NegotiateServiceCredential = false;

                EndpointIdentity identity = new DnsEndpointIdentity("B2BCurrentUserService");

                var factory = new ChannelFactory<IBusinessToBusinessCurrentUser>(clientBinding, new EndpointAddress(new Uri(uri), identity));
                factory.Credentials.ClientCertificate.SetCertificate(
                    "CN=B2BCurrentUserClient",
                    System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser,
                    System.Security.Cryptography.X509Certificates.StoreName.My);
                factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                var serviceCert = "B2BCurrentUserService";

                StoreLocation serviceLocation = StoreLocation.LocalMachine;
                CertHelper.TryGetCertLocation(serviceCert, out serviceLocation, true);

                factory.Credentials.ServiceCertificate.SetDefaultCertificate(
                    serviceLocation,
                    System.Security.Cryptography.X509Certificates.StoreName.My,
                    X509FindType.FindBySubjectName,
                    serviceCert);

                factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                var channel = factory.CreateChannel();
                Assert.AreEqual("hi", channel.TestMe("hi"));

                factory.Close();
            }
        }

        [TestMethod]
        public void BusinessToBusinessCert_WithProxy_CurrentUserCert()
        {
            string uri = "http://localhost/servicehelpers/b2b";
            using (var host = new ServiceHost(typeof(BusinessToBusinessCurrentUser), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IBusinessToBusinessCurrentUser), new WS2007HttpBinding(), uri);
                host.Open();

                ProxyFactory factory = new ProxyFactory();
                var proxy = factory.Proxy<IBusinessToBusinessCurrentUser>();
                Assert.AreEqual("hi", proxy.TestMe("hi"));
            }
        }

        [TestMethod]
        public void BusinessToBusinessCert_WithProxy_LocalMachineCert()
        {
            string uri = "http://localhost/servicehelpers/b2b_localmachine";
            using (var host = new ServiceHost(typeof(BusinessToBusinessLocalMachine), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IBusinessToBusinessLocalMachine), new WS2007HttpBinding(), uri);
                host.Open();

                ProxyFactory factory = new ProxyFactory();
                var proxy = factory.Proxy<IBusinessToBusinessLocalMachine>();
                Assert.AreEqual("hi", proxy.TestMe("hi"));
            }
        }

        [TestMethod]
        public void BusinessToBusinessCert_WithProxy_Errors()
        {
            string uri = "http://localhost/servicehelpers/b2b_noerrormasking";
            using (var host = new ServiceHost(typeof(BusinessToBusinessNoErrorMasking), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IBusinessToBusinessNoErrorMasking), new WS2007HttpBinding(), uri);
                host.Open();

                try
                {
                    ProxyFactory factory = new ProxyFactory();
                    var proxy = factory.Proxy<IBusinessToBusinessNoErrorMasking>();
                    Assert.AreEqual("hi", proxy.TestMe("hi"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        [TestMethod]
        public void BusinessToBusinessCert_WithProxy_CachedProxy()
        {
            string uri = "http://localhost/servicehelpers/b2b_Cached";
            using (var host = new ServiceHost(typeof(BusinessToBusinessCached), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IBusinessToBusinessCached), new WS2007HttpBinding(), uri);
                host.Open();

                ProxyFactory factory = new ProxyFactory();
                var proxy = factory.Proxy<IBusinessToBusinessCached>();
                Assert.AreEqual("hi", proxy.TestMe("hi"));
                Assert.AreEqual("hi", proxy.TestMe("hi"));
            }
        }

        [TestMethod]
        public void BusinessToBusinessCert_NoSecurityForNamedPipes()
        {
            ProxyFactory factory = new ProxyFactory();
            var proxy = factory.Proxy<IBusinessToBusinessPipe>();
            Assert.AreEqual("hi", proxy.TestMe("hi"));           
        }

        [TestMethod]
        public void BusinessToBusinessCert_Tcp()
        {
            string uri = "net.tcp://localhost:10095/servicehelpersTCP";
            using (var host = new ServiceHost(typeof(BusinessToBusinessTcp), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IBusinessToBusinessTcp), new NetTcpBinding(), uri);
                host.Open();

                ProxyFactory factory = new ProxyFactory();
                var proxy = factory.Proxy<IBusinessToBusinessTcp>();
                Assert.AreEqual("hi", proxy.TestMe("hi"));
            }
        }

        [TestMethod]
        public void BusinessToBusinessCert_Tcp_ProxyOnly()
        {
            string uri = "net.tcp://localhost:10095/servicehelpersTCP";
            using (var host = new ServiceHost(typeof(BusinessToBusinessTcp), new Uri(uri)))
            {
                var serviceBinding = new NetTcpBinding(SecurityMode.Message);
                serviceBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                host.Credentials.ServiceCertificate.SetCertificate(
                    "CN=B2BCurrentUserService",
                    System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser,
                    System.Security.Cryptography.X509Certificates.StoreName.My);
                host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                host.AddServiceEndpoint(typeof(IBusinessToBusinessTcp), serviceBinding, uri);

                host.Open();

                ProxyFactory factory = new ProxyFactory();
                var proxy = factory.Proxy<IBusinessToBusinessTcp>();
                Assert.AreEqual("hi", proxy.TestMe("hi"));               
            }
        }

        [TestMethod]
        public void BusinessToBusinessCert_BasicHttp()
        {
            string uri = "http://localhost/servicehelpers";
            using (var host = new ServiceHost(typeof(BusinessToBusiness1), new Uri(uri)))
            {
                host.AddServiceEndpoint(typeof(IBusinessToBusiness1), new BasicHttpBinding(), uri);
                host.Open();


                // raw proxy
                var clientBinding = new WS2007HttpBinding(SecurityMode.Message);
                clientBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                clientBinding.Security.Message.EstablishSecurityContext = false;

                EndpointIdentity identity = new DnsEndpointIdentity("RawTcpServiceCert1");

                var factory = new ChannelFactory<IBusinessToBusiness1>(clientBinding, new EndpointAddress(new Uri(uri), identity));
                factory.Credentials.ClientCertificate.SetCertificate(
                    "CN=RawTcpClientCert1",
                    System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    System.Security.Cryptography.X509Certificates.StoreName.My);
                factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                var channel = factory.CreateChannel();
                Assert.AreEqual("hi", channel.TestMe("hi"));

                factory.Close();
            }
        }

        [TestMethod]
        public void BusinessToBusinessCert_OverrideToInProc()
        {
            var factory = new ProxyFactory();
            var proxy = factory.Proxy<IBusinessToBusinessInProc>();
            Assert.AreEqual("hi", proxy.TestMe("hi"));
        }

    }
}
