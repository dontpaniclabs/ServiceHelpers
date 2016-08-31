using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace DontPanic.Helpers
{
    public static class CertHelper
    {
        public static bool TryGetCertLocation(string name, out StoreLocation location, bool checkPrivateKey)
        {
            var result = false;
            location = StoreLocation.LocalMachine;

            result = HasCert(name, StoreLocation.CurrentUser, StoreName.My, checkPrivateKey);
            if (result)
                location = StoreLocation.CurrentUser;
            else
                result = HasCert(name, StoreLocation.LocalMachine, StoreName.My, checkPrivateKey);

            return result;
        }

        public static bool HasCert(string name, StoreLocation location, StoreName storeName, bool checkPrivateKey)
        {
            var result = false;

            try
            {
                X509Store store = new X509Store(storeName, location);
                store.Open(OpenFlags.OpenExistingOnly);
                var findResult = store.Certificates.Find(X509FindType.FindBySubjectName, name, false);
                if (findResult != null && findResult.Count > 0)
                {
                    if (!checkPrivateKey || findResult[0].HasPrivateKey)
                        result = true;
                }
            }
            catch
            { }

            return result;
        }
    }

}
