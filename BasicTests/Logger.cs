    using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DontPanic.Helpers;
using System.Diagnostics;

namespace BasicTests
{
    /// <summary>
    /// Summary description for Logger
    /// </summary>
    [TestClass]
    public class Logger
    {
        public Logger()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Logger_Basic()
        {
            ProxyFactory factory = new ProxyFactory();
            Assert.IsNotNull(LoggerCache.Logger);

            LoggerCache.Logger.Log("test");
            LoggerCache.Logger.LogCall("test");
            LoggerCache.Logger.LogException("test", new InvalidOperationException("test"));
        }

        [TestMethod]
        public void Logger_Raw()
        {
           
        }

        class TProxyFactory : ProxyFactory
        {
            public void TestLog(string message)
            {
                Log(message);
            }
        }

        [TestMethod]
        public void Logger_ProxyFactory()
        {
            var proxy = new TProxyFactory();
            proxy.TestLog("hi");
        }
    }
}
