using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using DontPanic.Helpers;

namespace BasicTests
{
    [ServiceContract]
    public interface IMyNestedBase
    {
        [OperationContract]
        void Test1();
    }

    [ServiceContract]
    [InProc(typeof(MyNested))]
    [ExceptionBehavior]
    public interface IMyNested : IMyNestedBase
    {
        [OperationContract]
        void Test2();
    }

    public class MyNested : ServiceBase, IMyNested
    {
        public void Test2()
        {
            throw new Exception("TEST2");
        }

        public void Test1()
        {
            throw new Exception("TEST1");            
        }
    }

}
