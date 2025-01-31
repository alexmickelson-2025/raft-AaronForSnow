using NSubstitute;
using NSubstitute.Core.Arguments;
using RaftClassLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RaftTests;

public class LoggingTests
{
	// 1. when a leader receives a client command the leader sends the log entry in the next appendentries RPC to all nodes
	[Fact]
	public async Task WhenLeaderGetsCommandFromClientItAddsLogToNextHeartBeat()
	{
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		testServer.State = ServerState.Leader;
		testServer.Term = 1;
		await testServer.ClientRequestAsync("my request");
		Thread.Sleep(65);
		await testServer.StopAsync();
		LogEntry entry = new LogEntry(1, Operation.Default, "my request");
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.entry == "HB"));
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.newLogs.Count == 1));
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.term == 1)); 
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.newLogs[0] == entry)); 
	}
	// 2. when a leader receives a command from the client, it is appended to its log
	[Fact]
	public async Task WhenLeaderGetsCommandFromClientItAddsTOLog()
	{
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		testServer.State = ServerState.Leader;
		testServer.Term = 1;
		await testServer.ClientRequestAsync("my request for This Test");
		await testServer.StopAsync();
		LogEntry entry = new LogEntry(1, Operation.Default, "my request for This Test");
		Assert.Equal("my request for This Test", testServer.Log[0].uniqueValue);
	}
	// 3. when a node is new, its log is empty]
	[Fact]
	public void WhenNodeIsNewLogIsEmpty()
	{
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		Assert.Empty(testServer.Log);
	}
	// 4. when a leader wins an election, it initializes the nextIndex for each follower to the index just after the last one it its log
	[Fact]
	public async Task WhenLeaderElectedItMatchesNextIndexForEachFollowerToItsLog()
	{
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
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
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
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
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
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
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		testServer.Log.Add(new LogEntry(1,Operation.None, "first"));
		testServer.commitIndex = 1;
		await testServer.AppendEntriesAsync(new AppendEntry(1, "REQUEST COMMIT INDEX", 2, Operation.None, 3, new List<LogEntry>(), 0));
		//fake1.Received(1).AppendEntries(new AppendEntry(3, "COMMIT INDEX RESPONCE", 2, Operation.None, 1, new List<LogEntry>(), 1));
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.commitedIndex == 1));
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.nextIndex == 1));
		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.entry == "COMMIT INDEX RESPONCE"));
	}
	// 5. leaders maintain an "nextIndex" for each follower that is the index of the next log entry the leader will send to that follower
	[Fact]
	public void LeadersTrackNextIndexes()
	{
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		Assert.Equal(testServer.nextIndexes.Count, testServer.OtherServers.Count);
	}
	// 6. Highest committed index from the leader is included in AppendEntries RPC's
	[Fact]
	public async Task CommittedIndexIncludedInLeaderAppendEntriesHeartBeat()
	{
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		testServer.State = ServerState.Leader;
		Thread.Sleep(60);
		await fake1.Received().AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.commitedIndex == -1));//none, commited yet
	}
	// 7. entry index  //I assume this is the next index. the index of the next expected entry. It will be the entry you should expect if you add the list given in the append entry
	[Fact]
	public async Task NextIndexIncludedInAppendEntries()
	{
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		testServer.State = ServerState.Leader;
		Thread.Sleep(60);
		await fake1.Received().AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.nextIndex == 0));//none, commited yet
	}
	// 8. when a leader receives a majority responses from the clients after a log replication heartbeat,
	//		the leader sends a confirmation response to the client
	[Fact]
	public async Task LeaderConfirmsAppendEntriesResponce()
	{
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		testServer.State = ServerState.Leader;
		testServer.Term = 1;
		testServer.Log.Add(new LogEntry(1,Operation.Default, "one"));
		testServer.commitIndex = -1;
		await testServer.ConfirmAsync(1,1,0);//now has majority
		Thread.Sleep(65);
		await fake1.Received(1).ConfirmAsync(1, 3, 0); //client now knows it was recived
	}
	// 9. given a leader node, when a log is committed, it applies it to its internal state machine
	[Fact]
	public async Task LeaderCommitsLogThenAppliesToStateMachine()
	{
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		testServer.State = ServerState.Leader;
		testServer.Term = 1;
		testServer.Log.Add(new LogEntry(1, Operation.Default, "one"));
		testServer.commitIndex = -1;
		await testServer.ConfirmAsync(1, 1, 0);//now has majority
		Thread.Sleep(65);
		Assert.Equal(0, testServer.commitIndex);
		Assert.Equal("one", testServer.StateMachineDataBucket);
	}
	// 10. when a follower receives a valid heartbeat, it increases its commitIndex to match the commit index of the heartbeat
	//		1. reject the heartbeat if the previous log index / term number does not match your log
	[Fact]
	public async Task FollowerIncreasesCommitIndexToMatchLeaderAppendEntry()
	{
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		testServer.Log = [
			new LogEntry(1,Operation.None, "first"), //0
			new LogEntry(1,Operation.None, "second"), //1
			new LogEntry(2,Operation.None, "third"),//2
			new LogEntry(2,Operation.None, "fourth"),//3
			new LogEntry(2,Operation.None, "fith"),//4
		];
		testServer.commitIndex = 1;
		testServer.Term = 3;
		List<LogEntry> logEntries = new List<LogEntry>();
		await testServer.AppendEntriesAsync(new AppendEntry(1, "HB", 3, Operation.None, 3,logEntries, 5));
		Assert.Equal(3, testServer.commitIndex);
		Assert.Equal("thirdfourth", testServer.StateMachineDataBucket);
	}
	[Fact]
	public async Task FollowerDoesNotIncreasesCommitIndexToMatchLeaderAppendEntryWhenTermTooHigh()
	{
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		testServer.Log = [
			new LogEntry(1,Operation.None, "first"), //0
			new LogEntry(1,Operation.None, "second"), //1
			new LogEntry(2,Operation.None, "third"),//2
			new LogEntry(2,Operation.None, "fourth"),//3
			new LogEntry(2,Operation.None, "fith"),//4
		];
		testServer.commitIndex = 1;
		testServer.Term = 2;
		List<LogEntry> logEntries = new List<LogEntry>();
		await testServer.AppendEntriesAsync(new AppendEntry(1, "HB", 4, Operation.None, 3, logEntries, 5));
		Assert.Equal(1, testServer.commitIndex);
		Assert.Equal("", testServer.StateMachineDataBucket);
	}
	//[Fact]
	//public async Task FollowerDoesNotIncreasesCommitIndexToMatchLeaderAppendEntryWhenNextIndexTooHigh()
	//{
	//	testServer.Log = [
	//		new LogEntry(1,Operation.None, "first"), //0
	//		new LogEntry(1,Operation.None, "second"), //1
	//		new LogEntry(2,Operation.None, "third"),//2
	//		new LogEntry(2,Operation.None, "fourth"),//3
	//		new LogEntry(2,Operation.None, "fith"),//4
	//	];
	//	testServer.commitIndex = 1;
	//	testServer.Term = 3;
	//	List<LogEntry> logEntries = new List<LogEntry>();
	//	await testServer.AppendEntriesAsync(new AppendEntry(1, "HB", 3, Operation.None, 3, logEntries, 7));
	//	Assert.Equal(1, testServer.commitIndex);
	//	Assert.Equal("", testServer.StateMachineDataBucket);
	//}

	// 11. When sending an AppendEntries RPC, the leader includes the index and term of the entry in its log that immediately precedes the new entries
	//		1. If the follower does not find an entry in its log with the same index and term, then it refuses the new entries
	//          1. term must be same or newer
	//			2. if index is greater, it will be decreased by leader
	//			3. if index is less, we delete what we have
	//		2. if a follower rejects the AppendEntries RPC, the leader decrements nextIndex and retries the AppendEntries RPC

	// 12. when a leader sends a heartbeat with a log, but does not receive responses from a majority of nodes, the entry is uncommitted
	// 13. if a leader does not response from a follower, the leader continues to send the log entries in subsequent heartbeats 
	// 14. if a leader cannot commit an entry, it does not send a response to the client
	// 15. if a node receives an appendentries with a logs that are too far in the future from your local state, you should reject the appendentries
	// 16. if a node receives and appendentries with a term and index that do not match, you will reject the appendentry until you find a matching log 



}


