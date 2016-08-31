using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace DontPanic.Helpers
{
    public interface IProxyFactory
    {
        // Override default proxy implemetation
        void AddProxyOverride<I, T>(T proxy)
            where T : I
            where I : class;

        // Override default proxy implemetation
        void AddProxyOverride<I>(I proxy)
            where I : class;

        // Clear all proxy overrides.
        void ClearProxyOverrides();

        // Current best method to call a WCF method with no lambda.
        I Proxy<I>() where I : class;

        I Proxy<I>(string endpointOverrideAddress) where I : class;

        // Helper methods for current version.
        object CallMethod<I>(string name, object[] parameters, string endpointOverrideAddress) where I : class;

        // Use a lambda. Still ok.
        void Call<I>(WcfCall<I> call) where I : class;

        // Use a lambda. Still ok.
        void Call<I>(WcfCall<I> call, string endpointOverrideAddress) where I : class;

        // Programmatically set options for particular proxies
        void AddEndpointAddressOverride<I>(Uri uri);
    }
}
