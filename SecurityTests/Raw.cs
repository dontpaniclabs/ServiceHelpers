using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace SecurityTests
{
    [ServiceContract]
    public interface IRawTcpCertService
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class RawTcpCertService : IRawTcpCertService
    {

        public string TestMe(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    public interface IRawTcpCertServiceConfig
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class RawTcpCertServiceConfig : IRawTcpCertServiceConfig
    {

        public string TestMe(string input)
        {
            return input;
        }
    }

    [TestClass]
    public class Raw
    {
        [TestInitialize]
        [TestCleanup]
        public void Init()
        {
            ConfigureTestCerts.Configure();
        }

        [TestMethod]
        public void Raw_TcpCert()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string uri = "net.tcp://localhost:10095/servicehelpersTCP";
            using (var host = new ServiceHost(typeof(RawTcpCertService), new Uri(uri)))
            {
                var serviceBinding = new NetTcpBinding(SecurityMode.Message);
                serviceBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                host.Credentials.ServiceCertificate.SetCertificate(
                    "CN=RawTcpServiceCert1",
                    System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    System.Security.Cryptography.X509Certificates.StoreName.My);
                host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                host.AddServiceEndpoint(typeof(IRawTcpCertService), serviceBinding, uri);

                host.Open();

                var clientBinding = new NetTcpBinding(SecurityMode.Message);
                clientBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;

                EndpointIdentity identity = new DnsEndpointIdentity("RawTcpServiceCert1");

                var factory = new ChannelFactory<IRawTcpCertService>(clientBinding, new EndpointAddress(new Uri(uri), identity));
                factory.Credentials.ClientCertificate.SetCertificate(
                    "CN=RawTcpClientCert1",
                    System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    System.Security.Cryptography.X509Certificates.StoreName.My);
                factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                var channel = factory.CreateChannel();
                Assert.AreEqual("hi", channel.TestMe("hi"));

                factory.Close();
            }

            sw.Stop();
            Trace.WriteLine("total time = " + sw.ElapsedMilliseconds);
        }

        [TestMethod]
        public void Raw_HttpCert()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string uri = "http://localhost/servicehelpers";
            using (var host = new ServiceHost(typeof(RawTcpCertService), new Uri(uri)))
            {
                var serviceBinding = new WS2007HttpBinding(SecurityMode.Message);
                serviceBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                host.Credentials.ServiceCertificate.SetCertificate(
                    "CN=RawTcpServiceCert1",
                    System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    System.Security.Cryptography.X509Certificates.StoreName.My);
                host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                host.AddServiceEndpoint(typeof(IRawTcpCertService), serviceBinding, uri);

                host.Open();

                var clientBinding = new WS2007HttpBinding(SecurityMode.Message);
                clientBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;

                EndpointIdentity identity = new DnsEndpointIdentity("RawTcpServiceCert1");

                var factory = new ChannelFactory<IRawTcpCertService>(clientBinding, new EndpointAddress(new Uri(uri), identity));
                factory.Credentials.ClientCertificate.SetCertificate(
                    "CN=RawTcpClientCert1",
                    System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    System.Security.Cryptography.X509Certificates.StoreName.My);
                factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                var channel = factory.CreateChannel();
                Assert.AreEqual("hi", channel.TestMe("hi"));

                factory.Close();
            }

            sw.Stop();
            Trace.WriteLine("total time = " + sw.ElapsedMilliseconds);
        }

        [TestMethod]
        public void Raw_HttpCert_SecondCallTime()
        {
            string uri = "http://localhost/servicehelpers";
            using (var host = new ServiceHost(typeof(RawTcpCertService), new Uri(uri)))
            {
                var serviceBinding = new WS2007HttpBinding(SecurityMode.Message);
                serviceBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                host.Credentials.ServiceCertificate.SetCertificate(
                    "CN=RawTcpServiceCert1",
                    System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    System.Security.Cryptography.X509Certificates.StoreName.My);
                host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                host.AddServiceEndpoint(typeof(IRawTcpCertService), serviceBinding, uri);

                host.Open();

                var clientBinding = new WS2007HttpBinding(SecurityMode.Message);
                clientBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;

                EndpointIdentity identity = new DnsEndpointIdentity("RawTcpServiceCert1");

                var factory = new ChannelFactory<IRawTcpCertService>(clientBinding, new EndpointAddress(new Uri(uri), identity));
                factory.Credentials.ClientCertificate.SetCertificate(
                    "CN=RawTcpClientCert1",
                    System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    System.Security.Cryptography.X509Certificates.StoreName.My);
                factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                var channel = factory.CreateChannel();

                Assert.AreEqual("hi", channel.TestMe("hi"));

                Stopwatch sw = new Stopwatch();
                sw.Start();

                Assert.AreEqual("hi", channel.TestMe("hi"));

                sw.Stop();
                Trace.WriteLine("second call time = " + sw.ElapsedMilliseconds);

                factory.Close();
            }
        }

        private void Raw_HttpCert_Call()
        {
            string uri = "http://localhost/servicehelpers";
            using (var host = new ServiceHost(typeof(RawTcpCertService), new Uri(uri)))
            {
                var serviceBinding = new WS2007HttpBinding(SecurityMode.Message);
                serviceBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                host.Credentials.ServiceCertificate.SetCertificate(
                    "CN=RawTcpServiceCert1",
                    System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    System.Security.Cryptography.X509Certificates.StoreName.My);
                host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                host.AddServiceEndpoint(typeof(IRawTcpCertService), serviceBinding, uri);

                host.Open();

                var clientBinding = new WS2007HttpBinding(SecurityMode.Message);
                clientBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;

                EndpointIdentity identity = new DnsEndpointIdentity("RawTcpServiceCert1");

                var factory = new ChannelFactory<IRawTcpCertService>(clientBinding, new EndpointAddress(new Uri(uri), identity));
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
        public void Raw_HttpCert_SecondCallTime_NewProxy()
        {
            Raw_HttpCert_Call();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Raw_HttpCert_Call();
            sw.Stop();
            Trace.WriteLine("second call time = " + sw.ElapsedMilliseconds);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Raw_HttpCert_BadStore()
        {
            string uri = "http://localhost/servicehelpers";
            using (var host = new ServiceHost(typeof(RawTcpCertService), new Uri(uri)))
            {
                var serviceBinding = new WS2007HttpBinding(SecurityMode.Message);
                serviceBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                host.Credentials.ServiceCertificate.SetCertificate(
                    "CN=RawTcpServiceCert1",
                    System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser,
                    System.Security.Cryptography.X509Certificates.StoreName.My);
                host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                host.AddServiceEndpoint(typeof(IRawTcpCertService), serviceBinding, uri);

                host.Open();

                var clientBinding = new WS2007HttpBinding(SecurityMode.Message);
                clientBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;

                EndpointIdentity identity = new DnsEndpointIdentity("RawTcpServiceCert1");

                var factory = new ChannelFactory<IRawTcpCertService>(clientBinding, new EndpointAddress(new Uri(uri), identity));
                factory.Credentials.ClientCertificate.SetCertificate(
                    "CN=RawTcpClientCert1",
                    System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser,
                    System.Security.Cryptography.X509Certificates.StoreName.My);
                factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                var channel = factory.CreateChannel();
                Assert.AreEqual("hi", channel.TestMe("hi"));

                factory.Close();
            }
        }

        [TestMethod]
        public void Raw_HttpCert_Config()
        {
            string uri = "http://localhost/servicehelpers";
            using (var host = new ServiceHost(typeof(RawTcpCertServiceConfig), new Uri(uri)))
            {
                var serviceBinding = new WS2007HttpBinding(SecurityMode.Message);
                serviceBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                host.Credentials.ServiceCertificate.SetCertificate(
                    "CN=RawTcpServiceCert1",
                    System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    System.Security.Cryptography.X509Certificates.StoreName.My);
                host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                host.AddServiceEndpoint(typeof(IRawTcpCertServiceConfig), serviceBinding, uri);

                host.Open();

                var factory = new ChannelFactory<IRawTcpCertServiceConfig>("IRawTcpCertServiceConfig");

                var channel = factory.CreateChannel();
                Assert.AreEqual("hi", channel.TestMe("hi"));

                factory.Close();
            }
        }

        [TestMethod]
        public void Raw_HttpCert_NotFullName()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string uri = "http://localhost/servicehelpers";
            using (var host = new ServiceHost(typeof(RawTcpCertService), new Uri(uri)))
            {
                var serviceBinding = new WS2007HttpBinding(SecurityMode.Message);
                serviceBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
                host.Credentials.ServiceCertificate.SetCertificate(
                    "CN=RawTcpServiceCert_2",
                    System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    System.Security.Cryptography.X509Certificates.StoreName.My);
                host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                host.AddServiceEndpoint(typeof(IRawTcpCertService), serviceBinding, uri);

                host.Open();

                var clientBinding = new WS2007HttpBinding(SecurityMode.Message);
                clientBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;

                EndpointIdentity identity = new DnsEndpointIdentity("RawTcpServiceCert_2");

                var factory = new ChannelFactory<IRawTcpCertService>(clientBinding, new EndpointAddress(new Uri(uri), identity));
                factory.Credentials.ClientCertificate.SetCertificate(
                    StoreLocation.LocalMachine, 
                    StoreName.My, 
                    X509FindType.FindBySubjectName,
                    "RawTcpClientCert_"); // NOT FULL NAME
                    
                factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;

                var channel = factory.CreateChannel();
                Assert.AreEqual("hi", channel.TestMe("hi"));

                factory.Close();
            }

            sw.Stop();
            Trace.WriteLine("total time = " + sw.ElapsedMilliseconds);
        }

    }
}
