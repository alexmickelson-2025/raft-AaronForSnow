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

namespace RaftTests;

//[DisableParallelization]
public class RaftTests
{
    //  1. When a leader is active it sends a heart beat within 50ms.
    [Fact]
    public void LeaderSendsHeartBeats()
    {
        var testServer = new ServerAaron(1);
        testServer.State = ServerState.Leader;
        Thread.Sleep(65);
        testServer.Kill();
        Assert.True(testServer.Sentmessages.Count() >= 1);
    }
    //  2. When a node receives an AppendEntries from another node, then first node remembers that other node is the current leader.
    [Fact]
    public void AppendEntriesWillSetLeader()
    {
        var testServer = new ServerAaron(1,3);
        testServer.AppendEntries(senderID: 2, entry: "newEntrie", term: 3);
        testServer.Kill();
        Assert.Equal(2, testServer.LeaderId);
    }
    //  3. When a new node is initialized, it should be in follower state.
    [Fact]
    public void ServerStartsInFollowerMode()
    {
        var testServer = new ServerAaron(1);
        testServer.Kill();
        Assert.Equal(ServerState.Follower, testServer.State);
    }
    //  4. When a follower doesn't get a message for 300ms then it starts an election.
    [Fact]
    public void FollowerWillStartElection()
    {
        var testServer = new ServerAaron(1);
        Thread.Sleep(350);
        testServer.Kill();
        Assert.Contains("Election Request", testServer.Sentmessages);
    }
    //  5. When the election time is reset, it is a random value between 150 and 300ms.
    [Fact]
    public void ElectionTimeoutIsRandom()
    {
        List<double> t = new List<double>();
        for (int i = 0; i < 4; i++) {
            var testServer = new ServerAaron(1);
            testServer.Kill();
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
    public void ElectionWillBeginsWithHigherTerm()
    {
        var testServer = new ServerAaron(1);
        testServer.Term = 1;
        Thread.Sleep(350);
        testServer.Kill();
        Assert.True(testServer.Term > 1);
    }
    //  7. When a follower does get an AppendEntries message, it resets the election timer. (i.e.it doesn't start an election even after more than 300ms)
    [Fact]
    public void AppendEntriesResetsElectionTimer()
    {
        var testServer = new ServerAaron(1, 3);
        Thread.Sleep(100);
        testServer.AppendEntries(senderID: 2, entry: "newEntrie", term: 1);
        Thread.Sleep(100);
        testServer.AppendEntries(senderID: 2, entry: "newEntrie", term: 2);
        Thread.Sleep(100);
        testServer.AppendEntries(senderID: 2, entry: "newEntrie", term: 3);
        testServer.Kill();
        Assert.Equal(ServerState.Follower, testServer.State);
    }
    //  8. Given an election begins, when the candidate gets a majority of votes, it becomes a leader. (think of the easy case; can use two tests for single and multi-node clusters)
    [Fact]
    public void WhenCadidateGetMajorityVotesBecomesLeaderSingleNode()
    {
        var testServer = new ServerAaron(1); //default to 1 server
        Thread.Sleep(350);
        Assert.Equal(ServerState.Leader, testServer.State);
    }
    //  9. Given a candidate receives a majority of votes while waiting for unresponsive node, it still becomes a leader.
    [Fact]
    public void WhenCadidateGetMajorityVotesBecomesLeaderThreeNodes()
    {
        var testServer = new ServerAaron(1, 3);
        Thread.Sleep(350);
        testServer.ReciveVote(senderID: 3, true);
        Assert.Equal(ServerState.Leader, testServer.State);
    }
    // -9. A follower that has not voted and is in a later term than the candidate responds to a RequestForVoteRPC with no. (inverse of 9)
    [Fact]
    public void WhenCadidateDOESNOTGetMajorityVotesWILLNOTBecomesLeaderThreeNodes()
    {
        var testServer = new ServerAaron(1,3);
        Thread.Sleep(350);
        Assert.Equal(ServerState.Candidate, testServer.State);
    }
    // 10. A follower that has not voted and is in an earlier term responds to a RequestForVoteRPC with yes. (the reply will be a separate RPC)
    [Fact]
    public void WhenFolloewerAskedForVoteGetPositiveResponce()
    {
        var testServer = new ServerAaron(1, 3);
        testServer.RequestVote(2,2); // id, term
        Assert.Equal(ServerState.Follower, testServer.State);
        Assert.Equal(2, testServer.TermVotes.Last().RequesterId);
        Assert.Contains("Positive Vote", testServer.Sentmessages);
    }
    // 11. Given a candidate server that just became a candidate, it votes for itself.
    [Fact]
    public void WhenBecomesCadidateVotesForSelf()
    {
        var testServer = new ServerAaron(1);
        Thread.Sleep(350);
        Assert.Equal(1,testServer.Votes.First().VoterId);
    }
    // 12. Given a candidate, when it receives an AppendEntries message from a node with a later term, then candidate loses and becomes a follower.
    [Fact]
    public void AppendEntriesWillSetLeaderFromCadidateStateHigerTerms()
    {
        var testServer = new ServerAaron(1,3);
        Thread.Sleep(350);
        Assert.Equal(ServerState.Candidate, testServer.State);
        testServer.AppendEntries(senderID: 2, entry: "newEntrie", term: 30);
        testServer.Kill();
        Assert.Equal(ServerState.Follower, testServer.State);
        Assert.Equal(2, testServer.LeaderId);
    }
    // 13. Given a candidate, when it receives an AppendEntries message from a node with an equal term, then candidate loses and becomes a follower.
    [Fact]
    public void AppendEntriesWillSetLeaderFromCadidateStateEqualTerms()
    {
        var testServer = new ServerAaron(1, 3);
        Thread.Sleep(350);
        Assert.Equal(ServerState.Candidate, testServer.State);
        testServer.Term = 2; //should already be, but just in case
        testServer.AppendEntries(senderID: 2, entry: "newEntrie", term: 2);
        testServer.Kill();
        Assert.Equal(ServerState.Follower, testServer.State);
        Assert.Equal(2, testServer.LeaderId);
    }
    // 14. If a node receives a second request for vote for the same term, it should respond no. (again, separate RPC for response)
    [Fact]
    public void WhenFolloewerAskedForVoteSameTermForAnotherSendNegativeResponce()
    {
        var testServer = new ServerAaron(1, 3);
        testServer.RequestVote(2, 2); // id, term
        testServer.RequestVote(3, 2); // id, term
        Assert.Equal(ServerState.Follower, testServer.State);
        Assert.Equal(2, testServer.TermVotes.Last().RequesterId);
        Assert.Contains("Positive Vote", testServer.Sentmessages);
        Assert.Contains("Rejected Vote", testServer.Sentmessages);
    }
    // 15. If a node receives a second request for vote for a future term, it should vote for that node.
    [Fact]
    public void WhenFolloewerAskedForVoteSameTermAgainVoteAgain()
    {
        var testServer = new ServerAaron(1, 3);
        testServer.RequestVote(2, 2); // id, term
        testServer.RequestVote(2, 2); // id, term
        Assert.Equal(ServerState.Follower, testServer.State);
        Assert.Equal(2, testServer.TermVotes.Last().RequesterId);
        Assert.Contains("Positive Vote", testServer.Sentmessages);
        testServer.Sentmessages.Remove("Positive Vote");
        Assert.Contains("Positive Vote", testServer.Sentmessages);
        testServer.Sentmessages.Remove("Positive Vote");
        Assert.DoesNotContain("Positive Vote", testServer.Sentmessages);
    }
    // 16. Given a candidate, when an election timer expires inside of an election, a new election is started.
    [Fact]
    public void ElectionTimerExpiresInsideElectionStartsNewElection()
    {
        var testServer = new ServerAaron(1, 3);
        Thread.Sleep(350);
        Assert.Equal(ServerState.Candidate, testServer.State);
        Assert.Contains("Election Request", testServer.Sentmessages);
        testServer.Sentmessages.Remove("Election Request");
        Assert.DoesNotContain("Election Request", testServer.Sentmessages);
        Thread.Sleep(350);
        Assert.Equal(ServerState.Candidate, testServer.State);
        Assert.Contains("Election Request", testServer.Sentmessages);
        testServer.Kill();

    }
    // 17. When a follower node receives an AppendEntries request, it sends a response.
    [Fact]
    public void AppentEntriesRepliesWithSuccess()
    {
        var testServer = new ServerAaron(1,3);
        testServer.State = ServerState.Follower;
        testServer.AppendEntries(senderID: 2, entry: "newEntrie", term: 3);
        testServer.Kill();
        Assert.Contains("AppendReceived", testServer.Sentmessages);
    }
    // 18. Given a candidate receives an AppendEntries from a previous term, then rejects.
    [Fact]
    public void AppendEntriesWithLowerTermIsRejected()
    {
        var testServer = new ServerAaron(1, 3);
        testServer.LeaderId = 3;
        testServer.Term = 4;
        testServer.AppendEntries(senderID: 2, entry: "newEntrie", term: 1);
        testServer.Kill();
        Assert.Equal(ServerState.Follower, testServer.State);
        Assert.Equal(3, testServer.LeaderId);
        Assert.Contains("Leader is 3", testServer.Sentmessages); // act of rejecting
    }

}
// use NSubstitute to moq the other servers
//  1. When a leader is active it sends a heart beat within 50ms.
//  2. When a node receives an AppendEntries from another node, then first node remembers that other node is the current leader.
//  3. When a new node is initialized, it should be in follower state.
//  4. When a follower doesn't get a message for 300ms then it starts an election.
//  5. When the election time is reset, it is a random value between 150 and 300ms.
//    * between
//    * random: call n times and make sure that there are some that are different (other properties of the distribution if you like)
//  6. When a new election begins, the term is incremented by 1.
//    * Create a new node, store id in variable.
//    * wait 300 ms
//    * reread term(?)
//    * assert after is greater(by at least 1)
//  7. When a follower does get an AppendEntries message, it resets the election timer. (i.e.it doesn't start an election even after more than 300ms)
//  8. Given an election begins, when the candidate gets a majority of votes, it becomes a leader. (think of the easy case; can use two tests for single and multi-node clusters)
//  9. Given a candidate receives a majority of votes while waiting for unresponsive node, it still becomes a leader.
// -9. A follower that has not voted and is in a later term than the candidate responds to a RequestForVoteRPC with no. (inverse of 9)
// 10. A follower that has not voted and is in an earlier term responds to a RequestForVoteRPC with yes. (the reply will be a separate RPC)
// 11. Given a candidate server that just became a candidate, it votes for itself.
// 12. Given a candidate, when it receives an AppendEntries message from a node with a later term, then candidate loses and becomes a follower.
// 13. Given a candidate, when it receives an AppendEntries message from a node with an equal term, then candidate loses and becomes a follower.
// 14. If a node receives a second request for vote for the same term, it should respond no. (again, separate RPC for response)
// 15. If a node receives a second request for vote for a future term, it should vote for that node.
// 16. Given a candidate, when an election timer expires inside of an election, a new election is started.
// 17. When a follower node receives an AppendEntries request, it sends a response.
// 18. Given a candidate receives an AppendEntries from a previous term, then rejects.
// 19. When a candidate wins an election, it immediately sends a heart beat.
//  (testing persistence to disk will be a later assignment)