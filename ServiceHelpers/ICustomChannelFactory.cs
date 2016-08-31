using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DontPanic.Helpers
{
    public interface ICustomChannelFactory
    {
        IChannelWrapper<I> CreateChannel<I>(ClientEndpoint endpoint) where I : class;
    }
}
