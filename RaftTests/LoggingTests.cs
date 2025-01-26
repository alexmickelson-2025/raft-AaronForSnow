﻿using NSubstitute;
using NSubstitute.Core.Arguments;
using RaftClassLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftTests;

public class LoggingTests
{
	IServerAaron fake1;
	IServerAaron testServer;
	public LoggingTests()
	{

		Tools.SetUpThreeServers(out fake1, out testServer);
	}
	// 1. when a leader receives a client command the leader sends the log entry in the next appendentries RPC to all nodes
	[Fact]
	public void WhenLeaderGetsCommandFromClientItAddsLogToNextHeartBeat()
	{
		testServer.State = ServerState.Leader;
		testServer.Term = 1;
		testServer.ClientRequest("my request");
		Thread.Sleep(65);
		testServer.Kill();
		LogEntry entry = new LogEntry(1, Operation.Default, "my request");
		Assert.Single(testServer.Sentmessages);

		Assert.Equal("HB", testServer.Sentmessages[0]);
		fake1.Received(1).AppendEntries(Arg.Is<AppendEntry>(e => e.newLogs.Count == 1));
		fake1.Received(1).AppendEntries(Arg.Is<AppendEntry>(e => e.term == 1)); 
		fake1.Received(1).AppendEntries(Arg.Is<AppendEntry>(e => e.newLogs[0] == entry)); 
	}
	// 2. when a leader receives a command from the client, it is appended to its log
	[Fact]
	public void WhenLeaderGetsCommandFromClientItAddsTOLog()
	{
		testServer.State = ServerState.Leader;
		testServer.Term = 1;
		testServer.ClientRequest("my request for This Test");
		testServer.Kill();
		LogEntry entry = new LogEntry(1, Operation.Default, "my request for This Test");
		Assert.Empty(testServer.Sentmessages);
		Assert.Equal("my request for This Test", testServer.Log[0].uniqueValue);
	}
	// 3. when a node is new, its log is empty]
	[Fact]
	public void WhenNodeIsNewLogIsEmpty()
	{
		Assert.Empty(testServer.Log);
	}
}


