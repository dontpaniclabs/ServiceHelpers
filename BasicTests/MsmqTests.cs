using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Messaging;
using System.ServiceModel;
using DontPanic.Helpers;

namespace BasicTests
{
    [ServiceContract]
    public interface IQueuedCall
    {
        [OperationContract(IsOneWay=true)]
        void TestMe(string input);
    }

    public class QueuedCall : IQueuedCall
    {
        public void TestMe(string input)
        {
            Console.WriteLine(input);
        }
    }

    [TestClass]
    public class MsmqTests
    {
        const string queueName = @".\private$\servicehelperstestqueue";
        const string wcfQueueName = @"net.msmq://localhost/private/servicehelperstestqueue";

        [TestInitialize]
        public void Init()
        {
            
            if (!MessageQueue.Exists(queueName))
            {
                using (var queue = MessageQueue.Create(queueName, true))
                {

                }
            }
            
        }

        [TestMethod]
        public void Msmq_ProxyUsingConfig()
        {
            using (ServiceHost host = new ServiceHost(typeof(QueuedCall)))
            {
                host.AddServiceEndpoint(typeof(IQueuedCall), new NetMsmqBinding(), wcfQueueName);
                host.Open();

                var factory = new ProxyFactory();

                var proxy = factory.Proxy<IQueuedCall>();

                proxy.TestMe("Message");

                Console.WriteLine("press enter to exit");
                Console.ReadLine();
            }
        }
    }
}
