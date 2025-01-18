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
    public void FollowerRespondsToAppendEntriesToLeader()
    {
        IServerAaron fake1;
        IServerAaron testServer;
        Tools.SetUpThreeServers(out fake1, out testServer);

        testServer.AppendEntries(1, "tm", 2);
        fake1.Received(1).Confirm(2, 3); //term 2 from server 3
        //Tools.SleepElectionTimeoutBuffer(testServerFollower);
        //Assert.Contains("Election Request", testServer.Sentmessages);
    }

    
}
