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
