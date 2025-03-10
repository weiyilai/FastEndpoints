﻿using Microsoft.Extensions.Logging;
using TestCases.ClientStreamingTest;
using TestCases.CommandBusTest;
using TestCases.EventQueueTest;
using TestCases.ServerStreamingTest;

namespace RemoteProcedureCalls;

public class RpcTestBase : TestBase<Sut>
{
    protected readonly RemoteConnection Remote;

    protected RpcTestBase(Sut App)
    {
        var svcCollection = new ServiceCollection();
        svcCollection.AddSingleton<ILoggerFactory, LoggerFactory>();
        svcCollection.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        var svcProvider = svcCollection.BuildServiceProvider();
        Remote = new("http://testhost", svcProvider); //the actual hostname doesn't matter as we're replacing the httphandler below
        Remote.ChannelOptions.HttpHandler = App.CreateHandler();
        Remote.Register<TestCases.CommandBusTest.VoidCommand>();
        Remote.Register<SomeCommand, string>();
        Remote.Register<EchoCommand, EchoCommand>();
        Remote.RegisterServerStream<StatusStreamCommand, StatusUpdate>();
        Remote.RegisterClientStream<CurrentPosition, ProgressReport>();
        Remote.Subscribe<TestEventQueue, TestEventQueueHandler>();
        Remote.Subscribe<MyEvent, MyEventHandler>();
        Thread.Sleep(500);
    }
}