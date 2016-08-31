using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DontPanic.Helpers
{
    public interface ICachableChannel
    {
        string ChannelType { get; }
        int CacheCount { get; set; }
        string CacheKey { get; }
        bool InUse { get; set; }
        bool IsFaulted { get; }
        DateTime LastUse { get; set; }
    }

    public interface IChannelWrapper<I> : ICachableChannel
    {
        bool IsRealProxy { get; }

        I Instance { get; }       
      
        void Close();

        void Abort();
    }
}
