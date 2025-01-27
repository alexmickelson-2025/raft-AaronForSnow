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
        testServer.ReciveVote(1, true);
        Assert.Equal(ServerState.Leader, testServer.State);
        testServer.Stop();
        fake1.ClearReceivedCalls();
        Thread.Sleep(400);
        fake1.Received(0).AppendEntries(Arg.Any<AppendEntry>());
    }
}
