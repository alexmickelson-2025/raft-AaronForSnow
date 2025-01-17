﻿
namespace RaftClassLib;

public class ServerAaron : IServerAaron
{
    public ServerState State { get; set; }
    public List<string> Sentmessages { get; set; }
    public int ElectionTimer { get; set; }
    public bool IsLive { get; set; }
    public int LeaderId { get; set; }

    private Thread timer;
    public ServerAaron()
    {
        State = ServerState.Follower;
        Sentmessages = new List<string>();
        IsLive = true;
        timer = new Thread(advancetimer);
        timer.Start();
        CheckMessages();
    }
    private void advancetimer()
    {
        while (IsLive)
        {
            ElectionTimer += 10;
            Thread.Sleep(10);
        }
    }

    private void Respond(string message)
    {
        Sentmessages.Add(message);
    }
    private void CheckMessages()
    {
        while (IsLive)
        {
            Thread.Sleep(100);
            Respond("HB");
            if (Sentmessages.Count > 2)
            {
                IsLive = false;
            }
        }
    }
    public void Kill()
    {
        IsLive = false;
        timer.Join();
    }

    public void AppendEntries(int senderID, string entry, int term)
    {
        LeaderId = senderID;
        Respond("AppendReceived");
        ElectionTimer = 0;
    }

    public void AppendEntries()
    {
        throw new NotImplementedException();
    }
}
public enum ServerState
{
    Follower,
    Candadate,
    Leader
}
