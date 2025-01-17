
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
    public int ID {  get; set; }
    public int Term { get; set; }
    private int NumServers { get; set; }
    public List<Vote> Votes { get; set; }
    public List<TermVote> TermVotes { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    public ServerAaron(int id, int? numServers = 1)
#pragma warning restore CS8618
    {
        this.ID = id;
        this.NumServers = numServers ?? 1;
        this.Term = 1;
        TermVotes = new List<TermVote>();
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
        Term += Term;
        Respond("Election Request");
        Votes = new List<Vote>() { new Vote(1, true) };
        tallyVotes();
    }

    private void tallyVotes()
    {
        if (positiveVotes() > NumServers / 2)
        {
            State = ServerState.Leader;
        }
    }

    private int positiveVotes()
    {
        int count = 0;
        foreach (var vote in Votes)
        {
            if (vote.PositiveVote) {
                count++;
            }
        }
        return count;
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
        if (term > Term)
        {
            LeaderId = senderID;
            State = ServerState.Follower;
            Respond("AppendReceived");
            ElectionTimer.Stop();
            ElectionTimer.Start();
        }
    }

    public void ReciveVote(int senderID, bool positveVote)
    {
        Votes.Add(new Vote(senderID, positveVote));
        tallyVotes();
    }

    public void RequestVote(int requesterId, int term)
    {
        TermVotes.Add(new TermVote(requesterId, term));
        Respond("Positive Vote");
    }
}
public enum ServerState
{
    Follower,
    Candidate,
    Leader
}
public class Vote
{
    public int VoterId { get; set; }
    public bool PositiveVote { get; set; }
    public Vote(int voterId, bool positiveVote)
    {
        VoterId = voterId;
        PositiveVote = positiveVote;    
    }
}
