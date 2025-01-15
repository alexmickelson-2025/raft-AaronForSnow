﻿namespace RaftClassLib;

public class Server
{
    public ServerState State;
    public List<string> Sentmessages = new List<string>();
    public bool IsLive;
    public Server()
    {
        State = ServerState.Follower;
        IsLive = true;
        CheckMessages();
    }
    public void AppendEntries()
    {
        Sentmessages.Add("AppendReceived");
    }
    private void CheckMessages()
    {
        while (IsLive)
        {
            Thread.Sleep(100);
            Sentmessages.Add("HB");
            if (Sentmessages.Count > 2)
            {
                IsLive = false;
            }
        }
    }
}
public enum ServerState
{
    Follower,
    Candadate,
    Leader
}
