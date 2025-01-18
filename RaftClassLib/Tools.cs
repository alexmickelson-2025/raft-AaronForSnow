using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftClassLib;

public static class Tools
{
    public static void SleepElectionTimeoutBuffer(IServerAaron testServer)
    {
        Thread.Sleep((int)testServer.ElectionTimer.Interval + 30);
    }
    public static void SetUpThreeServers(out IServerAaron fake1, out IServerAaron testServer)
    {
        fake1 = Substitute.For<IServerAaron>();
        fake1.ID = 1;
        var fake2 = Substitute.For<IServerAaron>();
        fake2.ID = 2;
        testServer = new ServerAaron(3);
        fake1.OtherServers = [fake2, testServer];
        fake2.OtherServers = [fake1, testServer];
        testServer.OtherServers = [fake2, fake1];
        testServer.StartSim();
    }
}
