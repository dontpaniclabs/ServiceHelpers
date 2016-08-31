using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DontPanic.Helpers
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class ProxyMessageFilterAttribute : Attribute
    {
        private IMessageFilter _filterType = null;

        public ProxyMessageFilterAttribute(string filterType)
        {
            FilterTypeString = filterType;
        }

        public ProxyMessageFilterAttribute(Type filterType)
        {
            _filterType = Activator.CreateInstance(filterType) as IMessageFilter; 
        }

        public string FilterTypeString { get; set;}

        public IMessageFilter FilterType
        {
            get
            {
                if (_filterType == null)
                {
                    Type t = Type.GetType(FilterTypeString, false);
                    if (t != null)
                    {
                        _filterType = Activator.CreateInstance(t) as IMessageFilter;
                    }
                }
                return _filterType;
            }
        }
    }

    public interface IMessageFilter
    {
        bool PreFilter<I>(ProxyFactory factory, string method, object[] parameters, ref object result) where I : class;
        void PostFilter<I>(ProxyFactory factory, string method, object[] parameters, ref object result) where I : class;
    }

    static class MessageFilterHelper
    {
        public static bool PreFilter<I>(ProxyFactory factory, string methodName, object[] parameters, ref object result) where I : class
        {
            foreach (var filter in GetFilters(typeof(I)))
            {
                if (filter.PreFilter<I>(factory, methodName, parameters, ref result))
                {
                    return true;
                }                
            }
            return false;
        }

        public static void PostFilter<I>(ProxyFactory factory, string methodName, object[] parameters, ref object result) where I : class
        {
            foreach (var filter in GetFilters(typeof(I)))
            {
                filter.PostFilter<I>(factory, methodName, parameters, ref result);
            }
        }

        private static IMessageFilter[] GetFilters(Type interfaceType)
        {            
            List<IMessageFilter> result = new List<IMessageFilter>();

            var attributes = interfaceType.GetCustomAttributes(false);
            foreach (var attribute in attributes)
            {
                if (attribute is ProxyMessageFilterAttribute)
                {
                    var filter = (attribute as ProxyMessageFilterAttribute).FilterType;
                    if (filter != null)
                        result.Add(filter);
                }
            }

            return result.ToArray();
        }
    }
}
