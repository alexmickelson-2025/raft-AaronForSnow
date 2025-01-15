namespace RaftClassLib;

public class Server
{
    public ServerState State;
    public List<string> Sentmessages = new List<string>();
    public int ElectionTimer;
    public bool IsLive;
    private Thread timer;
    public Server()
    {
        State = ServerState.Follower;
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
    public void AppendEntries()
    {
        Respond("AppendReceived");
        ElectionTimer = 0;
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
}
public enum ServerState
{
    Follower,
    Candadate,
    Leader
}
