﻿
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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    public ServerAaron()
#pragma warning restore CS8618
    {
        State = ServerState.Follower;
        Sentmessages = new List<string>();
        startTimers();
        IsLive = true;
    }

    private void startTimers()
    {
        Random random = new Random();
        int interval = random.Next(150, 300);
        ElectionTimer = new Timer(interval);
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
