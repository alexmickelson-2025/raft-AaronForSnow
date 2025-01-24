using NSubstitute;
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
		testServer.ClientRequest("my request");
		Thread.Sleep(65);
		LogEntry entry = new LogEntry(1, Operation.Default, "my request");
		//AppendEntry entry = new AppendEntry();
		//AppendEntry recivedEntry = fake1.Sentmessages.;
		fake1.Received(1);
		fake1.Received(1).AppendEntries(Arg.Is<AppendEntry>(e => e.newLogs[0] == entry)); //term 2 from server 3
													  //fake1.Received(1).AppendEntries(Arg.Is<AppendEntry>(e => e.term == 1));
		fake1.Received(1).AppendEntries(Arg.Is<AppendEntry>(e => e.newLogs.Count == 1));

    }
}


