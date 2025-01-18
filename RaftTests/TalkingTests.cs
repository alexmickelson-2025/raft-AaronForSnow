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
    public TalkingTests() {
        
        Tools.SetUpThreeServers(out fake1, out testServer);
    }
    [Fact]
    public void FollowerRespondsToAppendEntriesToLeader()
    {
        testServer.AppendEntries(1, "tm", 2);
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
        fake1.Received(1).AppendEntries(3, "HB", 1);
    }
    //  1. When a leader is active it sends a heart beat within 50ms.
    [Fact]
    public void LeaderRecivesHeartBeatResponce()
    {
        testServer.AppendEntries(1, "HB", 1);
        fake1.Received(1).HBRecived(3);
    }
    [Fact]
    public void NetworkDelayModifiesAppendEntriesConferm()
    {
        IServerAaron fake1;
        IServerAaron testServer;
        Tools.SetUpThreeServers(out fake1, out testServer);
    }
    
}
