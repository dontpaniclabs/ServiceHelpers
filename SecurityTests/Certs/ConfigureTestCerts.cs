using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;

namespace SecurityTests
{
    static class ConfigureTestCerts
    {
        public static void Configure()
        {
            AddFromResource("RawTcpClientCert1.pfx", StoreLocation.LocalMachine);
            AddFromResource("RawTcpServiceCert1.pfx", StoreLocation.LocalMachine);

            AddFromResource("RawTcpClientCert_2.pfx", StoreLocation.LocalMachine);
            AddFromResource("RawTcpServiceCert_2.pfx", StoreLocation.LocalMachine);

            AddFromResource("B2BCurrentUserClient.pfx", StoreLocation.CurrentUser);
            AddFromResource("B2BCurrentUserService.pfx", StoreLocation.CurrentUser);

            AddFromResource("B2BLocalMachineClient.pfx", StoreLocation.CurrentUser);
            AddFromResource("B2BLocalMachineService.pfx", StoreLocation.CurrentUser);
        }

        public static void AddFromResource(string certName, StoreLocation location)
        {
            Remove(certName);

            var resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (var name in resources)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (name.Contains(certName))
                    {
                        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
                        {
                            byte[] buf = new byte[stream.Length];
                            stream.Read(buf, 0, (int)stream.Length);

                            var cert = new X509Certificate2(buf, "test", 
                                X509KeyStorageFlags.DefaultKeySet | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

                            Add(cert, "test", location);
                        }
                    }
                }
            }
        }

        private static void Add(X509Certificate2 cert, string password, StoreLocation location)
        {            
            X509Store store = new X509Store(StoreName.My, location);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close();           

            X509Store store2 = new X509Store(StoreName.TrustedPeople, location);
            store2.Open(OpenFlags.ReadWrite);
            store2.Add(cert);
            store.Close();
        }

        public static void Remove(string certName)
        {
            Remove(certName, StoreLocation.CurrentUser, StoreName.My);
            Remove(certName, StoreLocation.CurrentUser, StoreName.TrustedPeople);
            Remove(certName, StoreLocation.LocalMachine, StoreName.My);
            Remove(certName, StoreLocation.LocalMachine, StoreName.TrustedPeople);
        }

        public static void Remove(string certName, StoreLocation location, StoreName storeName)
        {
            var tmp = certName.Replace(".pfx", string.Empty).Replace(".cer", string.Empty);

            X509Store store = null;
            try
            {
                store = new X509Store(storeName, location);
                store.Open(OpenFlags.ReadWrite);
                var findResult1 = store.Certificates.Find(X509FindType.FindBySubjectName, tmp, false);
                if (findResult1 != null && findResult1.Count > 0)
                {
                    store.Remove(findResult1[0]);
                }
                store.Close();
            }
            catch
            { }
            finally
            {
                if (store != null)
                {

                }
            }
        }

    }
}
