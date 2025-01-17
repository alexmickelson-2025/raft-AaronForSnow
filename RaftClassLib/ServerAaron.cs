
namespace RaftClassLib;
using System.Timers;
public class ServerAaron : IServerAaron
{
    public ServerState State { get; set; }
    public List<string> Sentmessages { get; set; }
    public System.Timers.Timer ElectionTimer { get; set; }
    public System.Timers.Timer HBTimer { get; set; }
    public bool IsLive { get; set; }
    public int LeaderId { get; set; }
    public ServerAaron()
    {
        State = ServerState.Follower;
        Sentmessages = new List<string>();
        IsLive = true;
        ElectionTimer = new Timer(300);
        ElectionTimer.Elapsed += StartElection;
        ElectionTimer.AutoReset = true;
        ElectionTimer.Start();
        HBTimer = new Timer(50);
        HBTimer.Elapsed += sendHeartBeet;
        HBTimer.AutoReset = true;
        HBTimer.Start();
    }

    private void sendHeartBeet(object? sender, ElapsedEventArgs e)
    {
        if(State == ServerState.Leader)
        {
            Respond("HB");
        }
    }

    private void StartElection(object? sender, ElapsedEventArgs? e)
    {
        State = ServerState.Candidate;
        Respond("Election Request");
    }

    private void Respond(string message)
    {
        Sentmessages.Add(message);
    }
    public void Kill()
    {
        IsLive = false;
    }

    public void AppendEntries(int senderID, string entry, int term)
    {
        LeaderId = senderID;
        Respond("AppendReceived");
        ElectionTimer.Stop();
        ElectionTimer.Start();
    }

}
public enum ServerState
{
    Follower,
    Candidate,
    Leader
}
