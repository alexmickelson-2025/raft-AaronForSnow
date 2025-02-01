using NSubstitute;
using RaftClassLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RaftTests;

public class TalkingTests
{
    [Fact]
    public async Task FollowerRespondsToAppendEntriesToLeader()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		defaultEntry = new AppendEntry(1, "anything not HB", 2, Operation.None, 0, new List<LogEntry>());
		await testServer.AppendEntriesAsync(defaultEntry);
        await fake1.Received(1).ConfirmAsync(new ConfirmationDTO(2, 3)); //term 2 from server 3
    }
    [Fact]
    public async Task FollowerRespondsToVoteRequestPositive()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		await testServer.RequestVoteAsync(1, 2);
        await fake1.Received(1).ReciveVoteAsync(new ReceiveVoteDTO(3,true));
    }
    [Fact]
    public async Task FollowerRespondsToVoteRequestNegative()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		await testServer.RequestVoteAsync(2, 2);
        await testServer.RequestVoteAsync(1, 2);
        await fake1.Received(1).ReciveVoteAsync(new ReceiveVoteDTO(3, false));
    }
    //  1. When a leader is active it sends a heart beat within 50ms.
    [Fact]
    public async Task HeartBeatRecivedFromLeader()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		testServer.State = ServerState.Leader;
        Thread.Sleep(65);
        defaultEntry = new AppendEntry(1, "HB", 0, Operation.Default, 0, new List<LogEntry>());

		await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.entry == "HB"));
    }
    //  1. When a leader is active it sends a heart beat within 50ms.
    [Fact]
    public async Task LeaderRecivesHeartBeatResponce()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		defaultEntry = new AppendEntry(1, "HB",1, Operation.None, 0, new List<LogEntry>());
		await testServer.AppendEntriesAsync(defaultEntry);
        await fake1.Received(1).HBRecivedAsync(3);
    }
    [Fact] //Question for Instructor, Why does this test Fail?
    public async Task NetworkDelayModifiesAppendEntriesConfirm()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		//testServer.NetworkDelayModifier = 30;
		defaultEntry = new AppendEntry(1, "test", 2, Operation.None, 0, new List<LogEntry>());
		await testServer.AppendEntriesAsync(defaultEntry);
        //fake1.Received(0).Confirm(2, 3);
        Thread.Sleep(350);
        await fake1.Received(1).ConfirmAsync(new ConfirmationDTO(2, 3));
    }
    //  9. Given a candidate receives a majority of votes while waiting for unresponsive node, it still becomes a leader.
    [Fact]
	public async Task WhenCadidateGetMajorityVotesBecomesLeaderThreeNodes()
	{
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		testServer.Votes = new List<Vote>() { new Vote(3, true) };
		await testServer.ReciveVoteAsync(senderID: 1, true);
		await testServer.ReciveVoteAsync(senderID: 2, true);
		Assert.Equal(ServerState.Leader, testServer.State);
	}
	// 10. A follower that has not voted and is in an earlier term responds to a RequestForVoteRPC with yes. (the reply will be a separate RPC)
	[Fact]
	public async Task WhenFolloewerAskedForVoteGetPositiveResponce()
	{
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		await testServer.RequestVoteAsync(1, 2); // id, term
		Assert.Equal(ServerState.Follower, testServer.State);
		Assert.Equal(1, testServer.TermVotes.Last().RequesterId);
        await fake1.Received(1).ReciveVoteAsync(new ReceiveVoteDTO(3, true));
	}

}
