using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using DontPanic.Helpers;
using System.ServiceModel.Security;
using System.ServiceModel.Channels;

namespace SecurityTests
{
    [ServiceContract]
    public interface IRawTransport1
    {
        [OperationContract]
        string TestMe(string input);
    }

    public class RawTransport1 : ServiceBase, IRawTransport1
    {
        public string TestMe(string input)
        {
            return input;
        }
    }

    [TestClass]
    public class RawSSL
    {
        [TestMethod]
        public void RawSSL_HostOnly()
        {
            var binding = new CustomBinding()
            {
                Name = "EasySSL",                
            };
            binding.Elements.Add(new BinaryMessageEncodingBindingElement());
            binding.Elements.Add(new HttpsTransportBindingElement());
                        
            string uri = "https://localhost:9595/rawtransport";
            using (var host = new ServiceHost(typeof(RawTransport1)))
            {
                host.AddServiceEndpoint(typeof(IRawTransport1), binding, uri);

                host.Credentials.ServiceCertificate.SetCertificate(
                 "CN=RawTcpServiceCert1",
                 System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                 System.Security.Cryptography.X509Certificates.StoreName.My);
                host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;

                host.Open();

                //var factory = new ChannelFactory<IRawTransport1>(binding, new EndpointAddress(uri));

                //factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
                //factory.Credentials.ServiceCertificate.DefaultCertificate = host.Credentials.ServiceCertificate.Certificate;
                //var channel = factory.CreateChannel();

                //string a = channel.TestMe("hi");
            }

        }
    }
}
