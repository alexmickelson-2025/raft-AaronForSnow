using NSubstitute;
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
		testServer.ClientRequest();
		Thread.Sleep(65);
		AppendEntry entry = new AppendEntry();
		fake1.Received(1).AppendEntries(entry); //term 2 from server 3
	}
}


