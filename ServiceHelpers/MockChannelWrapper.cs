using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DontPanic.Helpers
{    
    public class MockChannelWrapper<I> : IChannelWrapper<I>
    {
        public I Instance { get; set; }
        
        public void Close()
        {            
        }

        public void Abort()
        {
        }

        public bool IsRealProxy
        {
            get { return false; }
        }

        public int CacheCount
        {
            get { return 0; }
            set { ; }
        }

        private string _cacheKey;
        public string CacheKey
        {
            get
            {
                if (_cacheKey == null)
                    _cacheKey = typeof(I).FullName;
                return _cacheKey;
            }
        }

        public bool InUse
        {
            get;
            set;
        }

        public DateTime LastUse { get; set; }

        public string ChannelType { get { return typeof(I).Name; } }

        public bool IsFaulted { get { return false; } }
    }
}
