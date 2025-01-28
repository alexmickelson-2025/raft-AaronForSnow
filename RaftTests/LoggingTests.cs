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
		testServer.nextIndexes = new List<int>() { 0,0 };
	}
	// 1. when a leader receives a client command the leader sends the log entry in the next appendentries RPC to all nodes
	[Fact]
	public async Task WhenLeaderGetsCommandFromClientItAddsLogToNextHeartBeat()
	{
		testServer.State = ServerState.Leader;
		testServer.Term = 1;
		await testServer.ClientRequestAsync("my request");
		Thread.Sleep(65);
		await testServer.StopAsync();
		LogEntry entry = new LogEntry(1, Operation.Default, "my request");
		Assert.Single(testServer.Sentmessages);

		Assert.Equal("HB", testServer.Sentmessages[0]);
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.newLogs.Count == 1));
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.term == 1)); 
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.newLogs[0] == entry)); 
	}
	// 2. when a leader receives a command from the client, it is appended to its log
	[Fact]
	public async Task WhenLeaderGetsCommandFromClientItAddsTOLog()
	{
		testServer.State = ServerState.Leader;
		testServer.Term = 1;
		await testServer.ClientRequestAsync("my request for This Test");
		await testServer.StopAsync();
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
	public async Task WhenLeaderElectedItMatchesNextIndexForEachFollowerToItsLog()
	{
		testServer.Log = [
			new LogEntry(1,Operation.None, "first"), //0
			new LogEntry(1,Operation.None, "second"), //1
			new LogEntry(2,Operation.None, "third"),//2
			new LogEntry(2,Operation.None, "fourth"),//3
			new LogEntry(2,Operation.None, "fith"),//4
		];
		testServer.commitIndex = 3;
		testServer.Term = 3;
		testServer.State = ServerState.Leader;
		//testServer.nextIndexes = [4, 5]; // first server needs one more log Second is caught up, should be set when call as COMMIT INDEX RESPONCE
		await testServer.AppendEntriesAsync(new AppendEntry(1, "COMMIT INDEX RESPONCE", 3, Operation.None, 3, new List<LogEntry>(), 4));
		Thread.Sleep(65); //so it sends at next heart beat
		await testServer.StopAsync();
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.commitedIndex == 3));
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.term == 3));
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.entry == "HB"));
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.nextIndex == 5));
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.newLogs.Count == 1));
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.newLogs[0].Term == 2));
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.newLogs[0].uniqueValue == "fith"));

	}
	[Fact]
	public async Task WhenElectedLeaderInitializesNextIndexList()
	{
		testServer.nextIndexes.Clear();
		Thread.Sleep(350);// will have gone into canidate state
		await testServer.ReciveVoteAsync(1, true);
		await testServer.ReciveVoteAsync(2, true);
		Assert.Equal(ServerState.Leader, testServer.State);
		Assert.Equal(0, testServer.nextIndexes[0]);
		Assert.Equal(0, testServer.nextIndexes[1]);
	}
	[Fact]
	public async Task WhenElectedLeaderAsksForCommitedIndexOfAllServers()//to set the nextIndex Value
	{
		testServer.nextIndexes.Clear();
		Thread.Sleep(350);// will have gone into canidate state
		await testServer.ReciveVoteAsync(1, true);
		await testServer.ReciveVoteAsync(2, true);
		Assert.Equal(ServerState.Leader, testServer.State);
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.entry == "REQUEST COMMIT INDEX"));
	}
	[Fact]
	public async Task RespondsToCommitIndexRequest()
	{
		testServer.Log.Add(new LogEntry(1,Operation.None, "first"));
		testServer.commitIndex = 1;
		await testServer.AppendEntriesAsync(new AppendEntry(1, "REQUEST COMMIT INDEX", 2, Operation.None, 3, new List<LogEntry>(), 0));
		//fake1.Received(1).AppendEntries(new AppendEntry(3, "COMMIT INDEX RESPONCE", 2, Operation.None, 1, new List<LogEntry>(), 1));
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.commitedIndex == 1));
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.nextIndex == 1));
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.entry == "COMMIT INDEX RESPONCE"));
	}
}


