using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using CloudSSLHost;
using System.Net;
using System.Net.Security;
using DontPanic.Helpers;

namespace CloudSSLClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
            delegate
            {
                return true;
            });
            
            //var factory = new ChannelFactory<IService1>("CloudSSLHost.Service1");

            //var binding = new WS2007HttpBinding(SecurityMode.TransportWithMessageCredential);
            //binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
            //binding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
            //binding.Security.Message.EstablishSecurityContext = false;

            //var factory = new ChannelFactory<IService1>(binding, new EndpointAddress("https://localhost/CloudSSLHost/Service1.svc"));

            //factory.Credentials.ClientCertificate.SetCertificate(
            //    System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, System.Security.Cryptography.X509Certificates.StoreName.My, System.Security.Cryptography.X509Certificates.X509FindType.FindBySubjectName, "EamClientCert_");

            var factory = new ProxyFactory();

            try
            {

                for (int i = 0; i < 10; i++)
                {
                    //var channel = factory.CreateChannel();
                    var channel = factory.Proxy<IService1>();
                    Console.WriteLine(channel.GetData(10));                    
                }
                //factory.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                //factory.Abort();
            }

            Console.WriteLine("press enter to exit");
            Console.ReadLine();
        }
    }
}
