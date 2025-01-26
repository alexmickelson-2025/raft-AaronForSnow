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
    IServerAaron fake1;
    IServerAaron testServer;
    AppendEntry defaultEntry; 
	public TalkingTests() {
        
        Tools.SetUpThreeServers(out fake1, out testServer);
        defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
	}
    [Fact]
    public void FollowerRespondsToAppendEntriesToLeader()
    {
        //testServer.StartSim();
		defaultEntry = new AppendEntry(1, "anything not HB", 2, Operation.None, 0, new List<LogEntry>());
		testServer.AppendEntries(defaultEntry);
        fake1.Received(1).Confirm(2, 3); //term 2 from server 3
    }
    [Fact]
    public void FollowerRespondsToVoteRequestPositive()
    {
        testServer.RequestVote(1, 2);
        fake1.Received(1).ReciveVote(3,true);
    }
    [Fact]
    public void FollowerRespondsToVoteRequestNegative()
    {
        testServer.RequestVote(2, 2);
        testServer.RequestVote(1, 2);
        fake1.Received(1).ReciveVote(3, false);
    }
    //  1. When a leader is active it sends a heart beat within 50ms.
    [Fact]
    public void HeartBeatRecivedFromLeader()
    {
        testServer.State = ServerState.Leader;
        Thread.Sleep(65);
        defaultEntry = new AppendEntry(1, "HB", 0, Operation.Default, 0, new List<LogEntry>());

		fake1.Received(1).AppendEntries(Arg.Is<AppendEntry>(e => e.entry == "HB"));
    }
    //  1. When a leader is active it sends a heart beat within 50ms.
    [Fact]
    public void LeaderRecivesHeartBeatResponce()
    {
		defaultEntry = new AppendEntry(1, "HB",1, Operation.None, 0, new List<LogEntry>());
		testServer.AppendEntries(defaultEntry);
        fake1.Received(1).HBRecived(3);
    }
    [Fact] //Question for Instructor, Why does this test Fail?
    public void NetworkDelayModifiesAppendEntriesConfirm()
    {
        //testServer.NetworkDelayModifier = 30;
		defaultEntry = new AppendEntry(1, "test", 2, Operation.None, 0, new List<LogEntry>());
		testServer.AppendEntries(defaultEntry);
        //fake1.Received(0).Confirm(2, 3);
        Thread.Sleep(350);
        fake1.Received(1).Confirm(2, 3);
    }
    //  9. Given a candidate receives a majority of votes while waiting for unresponsive node, it still becomes a leader.
    [Fact]
	public void WhenCadidateGetMajorityVotesBecomesLeaderThreeNodes()
	{
        testServer.Votes = new List<Vote>() { new Vote(3, true) };
		testServer.ReciveVote(senderID: 1, true);
		testServer.ReciveVote(senderID: 2, true);
		Assert.Equal(ServerState.Leader, testServer.State);
	}
	// 10. A follower that has not voted and is in an earlier term responds to a RequestForVoteRPC with yes. (the reply will be a separate RPC)
	[Fact]
	public void WhenFolloewerAskedForVoteGetPositiveResponce()
	{
		testServer.RequestVote(1, 2); // id, term
		Assert.Equal(ServerState.Follower, testServer.State);
		Assert.Equal(1, testServer.TermVotes.Last().RequesterId);
        fake1.Received(1).ReciveVote(3, true);
	}
}
