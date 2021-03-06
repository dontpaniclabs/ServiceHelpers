# ServiceHelpers
Don't Panic Lab Service Helpers

## Project Description
Service Helpers simplifies using WCF in a service oriented software system. Service Helpers makes working with WCF as easy as using any dependency injection framework.

## Why?
WCF is a great technology for implementing a Service Oriented Architecture, but the overhead of using WCF is a lot for programmers to deal with on a daily basis. Each WCF call involves creating a WCF channel proxy. The WCF channel proxy must be closed explicitly by the client, and if an exception occurs it must be aborted. Managing the lifecycle of the proxy is a lot of effort, and is very difficult to do correctly. This project simplifies using WCF and makes using WCF as easy as using a dependency injection framework. To accomplish this ServiceHelpers creates a thin proxy that lives between your caller and the WCF proxy. This thin proxy will be generated using Reflection.Emit. With the help of this thin proxy I can manage the lifecycle of the proxy in a very controlled manner. With the help of a little code, using WCF will be as easy as generating a proxy. In fact, WCF will not even be noticeable from the perspective the programmer.

## Sample Code
With DPL Service Helpers a WCF call looks like this

```
var factory = new ProxyFactory();
var proxy = factory.Proxy<IGenericContract>();
var result = proxy.Find(10);
```

```
string uri = "net.tcp://localhost:10095/servicehelpersTCP";

var channelFactory = new ChannelFactory<IExternalServiceTcp>(
	new NetTcpBinding(), new EndpointAddress(uri));
var proxy = channelFactory.CreateChannel();

try
{
	Assert.AreEqual("hi", proxy.TestMe("hi"));

	try
	{
		channelFactory.Close();
	}
	catch
	{ }
}
catch (Exception ex)
{
	if (channelFactory != null)
	{
		channelFactory.Abort();
	}
}
```

## Presentation
https://www.codeplex.com/Download?ProjectName=dplservicehelpers&DownloadId=264081

## Overview
![Overview Image](http://download-codeplex.sec.s-msft.com/Download?ProjectName=dplservicehelpers&DownloadId=264082)

## How To Use?

ProxyFactory greatly simplifies the usage of WCF proxies. Before ProxyFactory a programmer had to manage the lifetime of the WCF proxy. With ProxyFactory you just program against a contract, no worries about error handling.

ProxyFactory actually generates a thin proxy "dynamically" that lives between the caller and the real WCF proxy. The thin proxy will actually send all calls through the ProxyFactory CallMethod member. This thin proxy is generated using Reflection.Emit.

With the help of ProxyFactory, in process calls and and external WCF calls both look the same. You can easily move a service from in process to external without much effort, just a configuration change.

```
ProxyFactory factory = new ProxyFactory();

// In Process Example
var proxy = factory.Proxy<IInProcess>();
proxy.TestMe("hi");

// External Example (notice that it is the same!!!
var proxy = factory.Proxy<IExternal>();
proxy.TestMe("hi");
```

## In Process Services

With in process services the WCF service is being hosted within the process of the caller. In process WCF services use named pipes for communication. The easiest way to call an in process service is to apply the InProc attribute to the contract.

```
    [ServiceContract]
    [InProc(typeof(MyService))]
    public interface IMyService
    {
        [OperationContract]
        string TestMe(string input);
    }
```

After applying the attribute you can easily call the WCF by just creating an instance of the "generated" thin proxy using ProxyFactory.

```
ProxyFactory factory = new ProxyFactory();
IMyService proxy = factory.Proxy<IMyService>();
proxy.TestMe("hi");
```

In Proc calls can also be configured to using an configuration section. The configuration section below configures the IConfigContract to be implemented using the BasicTests.ConfigContract service.

```
  <ServiceHelpers>
    <inproc>
      <endpoint
        contract="BasicTests.IConfigContract"
        implementation="BasicTests.ConfigContract, BasicTests" />              
    </inproc>
  </ServiceHelpers>
```

## Externally Hosted Services

Externally hosted WCF services can be easily called using the same proxy factory code. The example below creates a WCF service endpoint hosting using TCP. The endpoint can then be reached using the same thin proxy generated by ProxyFactory.

```
string uri = "http://localhost/servicehelpers";
using (var host = new ServiceHost(typeof(ExternalService), new Uri(uri)))
{
    host.AddServiceEndpoint(typeof(IExternalService), new WSHttpBinding(), uri);
    host.Open();

    var factory = new ProxyFactory();

    // Generate a thin proxy.
    var proxy = factory.Proxy<IExternalService>();

    // Call the TestMe method. This will proxy the messsage thru the ProxyFactory.CallMethod.
    Assert.AreEqual("hi", proxy.TestMe("hi"));
}
```
The configuration for the proxy can be easily handled with the custom DPL configuration section.

```
  <ServiceHelpers>
    <external>
      <endpoint
        contract="BasicTests.IExternalService"
        address="http://localhost/servicehelpers" />     
     </external>
  </ServiceHelpers>
```







