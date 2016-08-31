using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DontPanic.Helpers;

namespace NonStandard
{
    [TestClass]
    public class Logger
    {
        [TestMethod]
        public void Logger_NoConfig()
        {
            LoggerCache.Logger.Log("test");
            LoggerCache.Logger.LogCall("test");
            LoggerCache.Logger.LogException("test", new InvalidOperationException("test"));
        }
    }
}
