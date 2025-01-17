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
        var testServer = new ServerAaron();
        testServer.State = ServerState.Leader;
        Thread.Sleep(55);
        testServer.Kill();
        Assert.True(testServer.Sentmessages.Count() >= 1);
    }
    //  2. When a node receives an AppendEntries from another node, then first node remembers that other node is the current leader.
    [Fact]
    public void AppendEntriesWillSetLeader()
    {
        var testServer = new ServerAaron();
        testServer.AppendEntries(senderID: 2, entry: "newEntrie", term: 1);
        testServer.Kill();
        Assert.Equal(2, testServer.LeaderId);
    }
    //  3. When a new node is initialized, it should be in follower state.
    [Fact]
    public void ServerStartsInFollowerMode()
    {
        var testServer = new ServerAaron();
        testServer.Kill();
        Assert.Equal(ServerState.Follower, testServer.State);
    }
    //  4. When a follower doesn't get a message for 300ms then it starts an election.
    [Fact]
    public void FollowerWillStartElection()
    {
        var testServer = new ServerAaron();
        Thread.Sleep(350);
        testServer.Kill();
        Assert.Equal(ServerState.Candidate, testServer.State);
        Assert.Contains("Election Request", testServer.Sentmessages);
    }
    //  7. When a follower does get an AppendEntries message, it resets the election timer. (i.e.it doesn't start an election even after more than 300ms)
    [Fact]
    public void AppendEntriesResetsElectionTimer()
    {
        var testServer = new ServerAaron();
        Thread.Sleep(100);
        testServer.AppendEntries(senderID: 2, entry: "newEntrie", term: 1);
        Thread.Sleep(100);
        testServer.AppendEntries(senderID: 2, entry: "newEntrie", term: 2);
        Thread.Sleep(100);
        testServer.AppendEntries(senderID: 2, entry: "newEntrie", term: 3);
        testServer.Kill();
        Assert.Equal(ServerState.Follower, testServer.State);
    }
    // 17. When a follower node receives an AppendEntries request, it sends a response.
    [Fact]
    public void AppentEntriesRepliesWithSuccess()
    {
        var testServer = new ServerAaron();
        testServer.State = ServerState.Follower;
        testServer.AppendEntries(senderID: 2, entry: "newEntrie", term: 1);
        testServer.Kill();
        Assert.Contains("AppendReceived", testServer.Sentmessages);
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