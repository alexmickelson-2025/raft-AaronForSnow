using Newtonsoft.Json.Linq;
using RaftClassLib;
using System.Collections.Generic;
using System;
using System.Net.Security;
using static System.Collections.Specialized.BitVector32;
using static System.Formats.Asn1.AsnWriter;
using System.Xml.Linq;
using Xunit;
using Meziantou.Xunit;
using NSubstitute;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NSubstitute.ReceivedExtensions;

namespace RaftTests;

//[DisableParallelization]
public class ElectionTests
{
    private static void SleepElectionTimeoutBuffer(ServerAaron testServer)
    {
        Thread.Sleep((int)testServer.ElectionTimer.Interval + 30);
    }
    //  1. When a leader is active it sends a heart beat within 50ms.
    [Fact]
    public async Task LeaderSendsHeartBeats()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		testServer.State = ServerState.Leader;
        await Task.Delay(50);
        //Thread.Sleep(65);
        await fake1.Received().AppendEntriesAsync(Arg.Any<AppendEntry>());
        await fake1.Received().AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.entry == "HB"));
	}
    //  2. When a node receives an AppendEntries from another node, then first node remembers that other node is the current leader.
    [Fact]
    public async Task AppendEntriesWillSetLeader()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(2, "HB", 3, Operation.None, 0, new List<LogEntry>());
		await testServer.AppendEntriesAsync(defaultEntry);
        Assert.Equal(2, testServer.LeaderId);
    }
    //  3. When a new node is initialized, it should be in follower state.
    [Fact]
    public void ServerStartsInFollowerMode()
    {
        IServerAaron testServer = new ServerAaron(1);
        Assert.Equal(ServerState.Follower, testServer.State);
    }
    //  4. When a follower doesn't get a message for 300ms then it starts an election.
    [Fact]
    public async Task FollowerWillStartElection()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		Tools.SleepElectionTimeoutBuffer(testServer);
        await fake1.Received().RequestVoteAsync(3,2);
    }
//  5. When the election time is reset, it is a random value between 150 and 300ms.
//    * between
//    * random: call n times and make sure that there are some that are different (other properties of the distribution if you like)
  [Fact]
    public async Task ElectionTimeoutIsRandom()
    {
        List<double> t = new List<double>();
        for (int i = 0; i < 4; i++) {
            IServerAaron testServer = new ServerAaron(1);
            await testServer.StartSimAsync();
            Assert.True(testServer.ElectionTimer.Interval >= 150);
            Assert.True(testServer.ElectionTimer.Interval <= 300);
            t.Add(testServer.ElectionTimer.Interval);
        }
        Assert.False((t[0] == t[1]) && (t[0] == t[2]) && (t[0] == t[3]) && (t[1] == t[2]) && (t[1] == t[3]) && (t[2] == t[3]));
    }
    //  6. When a new election begins, the term is incremented by 1.
    //    * Create a new node, store id in variable.
    //    * wait 300 ms
    //    * reread term(?)
    //    * assert after is greater(by at least 1)
    [Fact]
    public async Task ElectionWillBeginsWithHigherTerm()
    {
        IServerAaron testServer = new ServerAaron(1);
        await testServer.StartSimAsync();
        testServer.Term = 1;
        Tools.SleepElectionTimeoutBuffer(testServer);
        Assert.True(testServer.Term > 1);
    }
    //  7. When a follower does get an AppendEntries message, it resets the election timer. (i.e.it doesn't start an election even after more than 300ms)
    [Fact]
    public async Task AppendEntriesResetsElectionTimer()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		Thread.Sleep(100);
        await testServer.AppendEntriesAsync(defaultEntry);
        Thread.Sleep(100);
        await testServer.AppendEntriesAsync(defaultEntry);
        Thread.Sleep(100);
        await testServer.AppendEntriesAsync(defaultEntry);
        Assert.Equal(ServerState.Follower, testServer.State);
    }
    //  8. Given an election begins, when the candidate gets a majority of votes, it becomes a leader. (think of the easy case; can use two tests for single and multi-node clusters)
    [Fact]
    public async Task WhenCadidateGetMajorityVotesBecomesLeaderSingleNode()
    {
        var testServer = new ServerAaron(1); //default to 1 server
        await testServer.StartSimAsync();
        Tools.SleepElectionTimeoutBuffer(testServer);
        Assert.Equal(ServerState.Leader, testServer.State);
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
		Tools.SleepElectionTimeoutBuffer(testServer);
        await testServer.ReciveVoteAsync(senderID: 3, true);
        Assert.Equal(ServerState.Leader, testServer.State);
    }
    // -9. A follower that has not voted and is in a later term than the candidate responds to a RequestForVoteRPC with no. (inverse of 9)
    [Fact]
    public async Task WhenCadidateDOESNOTGetMajorityVotesWILLNOTBecomesLeaderThreeNodes()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		Tools.SleepElectionTimeoutBuffer(testServer);
        Assert.Equal(ServerState.Candidate, testServer.State);
    }
    // 10. A follower that has not voted and is in an earlier term responds to a RequestForVoteRPC with yes. (the reply will be a separate RPC)
    [Fact]
    public async Task WhenFolloewerAskedForVoteGetPositiveResponce()
    {
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		await testServer.RequestVoteAsync(fake1.ID, 2); // id, term
        Assert.Equal(ServerState.Follower, testServer.State);
        Assert.Equal(fake1.ID, testServer.TermVotes.Last().RequesterId);
        await fake1.Received(1).ReciveVoteAsync(senderID: testServer.ID, true);
    }
    // 10.5 When I am a candidate I request votes from other servers
    [Fact]
    public async Task CadidateVotesRequestVotes()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		Thread.Sleep(350);
        await fake1.Received(1).RequestVoteAsync(testServer.ID, 2);
    }
		// 11. Given a candidate server that just became a candidate, it votes for itself.
	[Fact]
    public async Task WhenBecomesCadidateVotesForSelf()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		Tools.SleepElectionTimeoutBuffer(testServer);
        Assert.Equal(ServerState.Candidate, testServer.State);
        Assert.Equal(testServer.ID,testServer.Votes.First().VoterId);
    }
    // 12. Given a candidate, when it receives an AppendEntries message from a node with a later term, then candidate loses and becomes a follower.
    [Fact]
    public async Task AppendEntriesWillSetLeaderFromCadidateStateHigerTerms()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
        Thread.Sleep(350);
		Assert.Equal(ServerState.Candidate, testServer.State);
		defaultEntry = new AppendEntry(2, "HB", 30, Operation.None, 0, new List<LogEntry>());
		await testServer.AppendEntriesAsync(defaultEntry);
        await testServer.StopAsync();
        Assert.Equal(ServerState.Follower, testServer.State);
        Assert.Equal(2, testServer.LeaderId);
    }
    // 13. Given a candidate, when it receives an AppendEntries message from a node with an equal term, then candidate loses and becomes a follower.
    [Fact]
    public async Task AppendEntriesWillSetLeaderFromCadidateStateEqualTerms()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		Tools.SleepElectionTimeoutBuffer(testServer);
        Assert.Equal(ServerState.Candidate, testServer.State);
        testServer.Term = 2; //should already be, but just in case
		defaultEntry = new AppendEntry(2, "HB", 2, Operation.None, 0, new List<LogEntry>());
		await testServer.AppendEntriesAsync(defaultEntry);
        await testServer.StopAsync();
        Assert.Equal(ServerState.Follower, testServer.State);
        Assert.Equal(2, testServer.LeaderId);
    }
    // 14. If a node receives a second request for vote for the same term, it should respond no. (again, separate RPC for response)
    [Fact]
    public async Task WhenFolloewerAskedForVoteSameTermForAnotherSendNegativeResponce()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		await testServer.RequestVoteAsync(2, 2); // id, term
        await testServer.RequestVoteAsync(1, 2); // id, term
        Assert.Equal(ServerState.Follower, testServer.State);
        Assert.Equal(2, testServer.TermVotes.Last().RequesterId);
        await fake1.Received().ReciveVoteAsync(3, false);
    }
    // 15. If a node receives a second request for vote for a future term, it should vote for that node.
    [Fact]
    public async Task WhenFolloewerAskedForVoteSameTermAgainVoteAgain()
    {
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		await testServer.RequestVoteAsync(1, 2); // id, term
        await testServer.RequestVoteAsync(1, 2); // id, term
        Assert.Equal(ServerState.Follower, testServer.State);
        Assert.Equal(1, testServer.TermVotes.Last().RequesterId);
        await fake1.Received(2).ReciveVoteAsync(3, true);
    }
    // 16. Given a candidate, when an election timer expires inside of an election, a new election is started.
    [Fact]
    public async void ElectionTimerExpiresInsideElectionStartsNewElection()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		Tools.SleepElectionTimeoutBuffer(testServer);
        Assert.Equal(ServerState.Candidate, testServer.State);
        await fake1.Received(1).RequestVoteAsync(3, 2);
        Tools.SleepElectionTimeoutBuffer(testServer);
        Assert.Equal(ServerState.Candidate, testServer.State);
		await fake1.Received(1).RequestVoteAsync(3, 2);
    }
    // 17. When a follower node receives an AppendEntries request, it sends a response.
    [Fact]
    public async Task AppentEntriesRepliesWithSuccess()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		testServer.State = ServerState.Follower;
		await testServer.AppendEntriesAsync(defaultEntry);
        await testServer.StopAsync();
        fake1.Received(1);
    }
    // 18. Given a candidate receives an AppendEntries from a previous term, then rejects.
    [Fact]
    public async Task AppendEntriesWithLowerTermIsRejected()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		testServer.LeaderId = 1;
        testServer.Term = 4;
		await testServer.AppendEntriesAsync(defaultEntry);
        await testServer.StopAsync();
        Assert.Equal(ServerState.Follower, testServer.State);
        Assert.Equal(1, testServer.LeaderId);
        await fake1.Received().AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.entry == "Leader is 1"));
    }
    // 19. When a candidate wins an election, it immediately sends a heart beat.
    [Fact]
    public async Task WhenCadidateBecomdesLeaderImmediateSendHeartBeet()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		Tools.SleepElectionTimeoutBuffer(testServer);
        await testServer.ReciveVoteAsync(senderID: 3, true);
        Assert.Equal(ServerState.Leader, testServer.State);
        await fake1.Received().AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.entry == "HB"));
    }
}
// use NSubstitute to moq the other servers
//  (testing persistence to disk will be a later assignment)