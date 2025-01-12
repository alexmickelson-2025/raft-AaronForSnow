using RaftClassLib;
using System.Net.Security;

namespace RaftTests
{
    public class RaftTests
    {
        [Fact]
        public void ServerStartsInFollowerMode()
        {
            var testServer = new Server();

            Assert.Equal(ServerState.Follower, testServer.State);
        }
    }
    // use NSubstitute to moq the other servers
}