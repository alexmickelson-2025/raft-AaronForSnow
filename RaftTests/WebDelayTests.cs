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
    // When node is a leader with an election loop, then they get paused, other nodes do not get heartbeat for 400ms
    [Fact]
    public async Task PausedLeaderWillNotSendHeartBeets()
    {
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		Thread.Sleep(330);
        await testServer.ReciveVoteAsync(1, true);
        Assert.Equal(ServerState.Leader, testServer.State);
        await testServer.StopAsync();
        fake1.ClearReceivedCalls();
        Thread.Sleep(400);
        await fake1.Received(0).AppendEntriesAsync(Arg.Any<AppendEntry>());
    }
    // When node is a leader with an election loop, the get paused,
    // other nodes do not get hearbeats fot 400 ms, then the get un-paused and dearbeats resume
    [Fact]
    public async Task ResumedLeaderWillSendHeartBeets()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		Thread.Sleep(330);
        await testServer.ReciveVoteAsync(1, true);
        Assert.Equal(ServerState.Leader, testServer.State);
        await testServer.StopAsync();
        fake1.ClearReceivedCalls();
        Thread.Sleep(400);
        await fake1.Received(0).AppendEntriesAsync(Arg.Any<AppendEntry>());
        await testServer.StartSimAsync();
        Thread.Sleep(65);
        await fake1.Received(1).AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.entry == "HB"));
    }
    // When a folloewer gets paused, it does not time out to become a cadidate
    [Fact]
    public async Task PausedFollowerDoesNotStartElectionTimeout()
	{
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
		await testServer.StopAsync();
        Thread.Sleep(400);
        Assert.Equal(ServerState.Follower, testServer.State);
    }
    // When a follower gets unpaused it will become a candidate given it has time for election time out again
    [Fact]
    public async Task PausedFollowerDoesWIllStartElectionTimeoutOnceResumed()
    {
		AppendEntry defaultEntry;
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		defaultEntry = new AppendEntry(1, "HB", 2, Operation.None, 0, new List<LogEntry>());
        Thread.Sleep(400);
        Assert.Equal(ServerState.Follower, testServer.State);
        await testServer.StartSimAsync();
        Thread.Sleep(350);
        Assert.Equal(ServerState.Candidate, testServer.State);

    }
}
