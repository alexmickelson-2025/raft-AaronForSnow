using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using RaftClassLib;

namespace RaftTests;

public class WebDelayTests
{
    IServerAaron fake1;
    IServerAaron testServer;
    public WebDelayTests()
    {
        Tools.SetUpThreeServers(out fake1, out testServer);
    }
    // When node is a leader with an election loop, then they get paused, other nodes do not get heartbeat for 400ms
    [Fact]
    public void PausedLeaderWillNotSendHeartBeets()
    {
        Thread.Sleep(330);
        testServer.ReciveVoteAsync(1, true);
        Assert.Equal(ServerState.Leader, testServer.State);
        testServer.StopAsync();
        fake1.ClearReceivedCalls();
        Thread.Sleep(400);
        fake1.Received(0).AppendEntriesAsync(Arg.Any<AppendEntry>());
    }
    // When node is a leader with an election loop, the get paused,
    // other nodes do not get hearbeats fot 400 ms, then the get un-paused and dearbeats resume
    [Fact]
    public void ResumedLeaderWillSendHeartBeets()
    {
        Thread.Sleep(330);
        testServer.ReciveVoteAsync(1, true);
        Assert.Equal(ServerState.Leader, testServer.State);
        testServer.StopAsync();
        fake1.ClearReceivedCalls();
        Thread.Sleep(400);
        fake1.Received(0).AppendEntriesAsync(Arg.Any<AppendEntry>());
        testServer.StartSimAsync();
        Thread.Sleep(65);
        fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.entry == "HB"));
    }
    // When a folloewer gets paused, it does not time out to become a cadidate
    [Fact]
    public void PausedFollowerDoesNotStartElectionTimeout()
    {
        testServer.StopAsync();
        Thread.Sleep(400);
        Assert.Equal(ServerState.Follower, testServer.State);
    }
    // When a follower gets unpaused it will become a candidate given it has time for election time out again
    [Fact]
    public void PausedFollowerDoesWIllStartElectionTimeoutOnceResumed()
    {
        testServer.StopAsync();
        Thread.Sleep(400);
        Assert.Equal(ServerState.Follower, testServer.State);
        testServer.StartSimAsync();
        Thread.Sleep(350);
        Assert.Equal(ServerState.Candidate, testServer.State);

    }
}
