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
		testServer.State = ServerState.Leader;
		testServer.Term = 1;
		testServer.ClientRequest("my request");
		Thread.Sleep(65);
		testServer.Stop();
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
		testServer.Stop();
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
	// 4. when a leader wins an election, it initializes the nextIndex for each follower to the index just after the last one it its log
	[Fact]
	public void WhenLeaderElectedItMatchesNextIndexForEachFollowerToItsLog()
	{
		testServer.Log = [
			new LogEntry(1,Operation.None, "first"),
			new LogEntry(1,Operation.None, "second"),
			new LogEntry(2,Operation.None, "third"),
			new LogEntry(2,Operation.None, "fourth"),
			new LogEntry(2,Operation.None, "fith"),
		];
		testServer.commitIndex = 3;
		testServer.State = ServerState.Leader;
		testServer.nextIndexes = [4, 5];
		// TODO: Finish This One
	}
	[Fact]
	public void WhenElectedLeaderInitializesNextIndexList()
	{
		testServer.nextIndexes.Clear();
		Thread.Sleep(350);// will have gone into canidate state
		testServer.ReciveVote(1, true);
		testServer.ReciveVote(2, true);
		Assert.Equal(ServerState.Leader, testServer.State);
		Assert.Equal(0, testServer.nextIndexes[0]);
		Assert.Equal(0, testServer.nextIndexes[1]);
	}
	[Fact]
	public void WhenElectedLeaderAsksForCommitedIndexOfAllServers()//to set the nextIndex Value
	{
		testServer.nextIndexes.Clear();
		Thread.Sleep(350);// will have gone into canidate state
		testServer.ReciveVote(1, true);
		testServer.ReciveVote(2, true);
		Assert.Equal(ServerState.Leader, testServer.State);
		fake1.Received(1).AppendEntries(Arg.Is<AppendEntry>(e => e.entry == "REQUEST COMMIT INDEX"));
	}
	[Fact]
	public void RespondsToCommitIndexRequest()
	{
		testServer.Log.Add(new LogEntry(1,Operation.None, "first"));
		testServer.commitIndex = 1;
		testServer.AppendEntries(new AppendEntry(1, "REQUEST COMMIT INDEX", 2, Operation.None, 3, new List<LogEntry>(), 0));
		//fake1.Received(1).AppendEntries(new AppendEntry(3, "COMMIT INDEX RESPONCE", 2, Operation.None, 1, new List<LogEntry>(), 1));
		fake1.Received(1).AppendEntries(Arg.Is<AppendEntry>(e => e.commitedIndex == 1));
		fake1.Received(1).AppendEntries(Arg.Is<AppendEntry>(e => e.nextIndex == 1));
		fake1.Received(1).AppendEntries(Arg.Is<AppendEntry>(e => e.entry == "COMMIT INDEX RESPONCE"));
	}
}


