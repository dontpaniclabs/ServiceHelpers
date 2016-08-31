using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DontPanic.Helpers
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class InProcAttribute : Attribute
    {
        public Type ServiceType { get; private set; }
        public bool NoWcf { get; private set; }
        public int CacheCount { get; private set; }

        /// <summary>
        /// Default verison of InProc attribute.
        /// </summary>
        /// <param name="type"></param>
        public InProcAttribute(Type type)
        {
            ServiceType = type;
        }

        public InProcAttribute(Type type, bool noWcf)
        {
            ServiceType = type;
            NoWcf = noWcf;
        }

        /// <summary>
        /// Use this constructor to enable proxy caching.
        /// </summary>
        /// <param name="type">Type to use to implement the contract</param>
        /// <param name="cacheCount">Cache count givs us the ability to cache the proxies.</param>
        public InProcAttribute(Type type, int cacheCount)
        {
            ServiceType = type;
            CacheCount = cacheCount;
        }

        public InProcAttribute(string typeString)
        {
            ServiceType = Type.GetType(typeString, false);
        }

        public InProcAttribute(string typeString, bool noWcf)
        {
            ServiceType = Type.GetType(typeString, false);
            NoWcf = noWcf;
        }
    }
}
