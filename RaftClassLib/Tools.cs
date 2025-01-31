using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftClassLib;

public static class Tools
{
    public static void SleepElectionTimeoutBuffer(ServerAaron testServer)
    {
        Thread.Sleep((int)testServer.ElectionTimer.Interval + 30);
    }
    public static void SetUpThreeServers(out IServerAaron fake1, out ServerAaron testServer)
    {
        fake1 = Substitute.For<IServerAaron>();
        fake1.ID = 1;
        var fake2 = Substitute.For<IServerAaron>();
        fake2.ID = 2;
        testServer = new ServerAaron(3);
        testServer.OtherServers = [fake2, fake1];
		testServer.nextIndexes = new List<int>() { 0, 0 };
    }
}
