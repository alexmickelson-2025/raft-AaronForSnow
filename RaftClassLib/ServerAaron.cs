
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
    private int NumServers { get => OtherServers.Count + 1; }
    public List<Vote> Votes { get; set; }
    public List<TermVote> TermVotes { get; set; }
    public List<IServerAaron> OtherServers { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    public ServerAaron(int id, int? numServers = 1)
#pragma warning restore CS8618
    {
        this.ID = id;
        this.Term = 1;
        TermVotes = new List<TermVote>();
        State = ServerState.Follower;
        Sentmessages = new List<string>();
        startTimers();
        IsLive = true;
        OtherServers = new List<IServerAaron>();
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

    private void sendHeartBeet(object? sender, ElapsedEventArgs? e)
    {
        if(State == ServerState.Leader)
        {
            Respond("HB");
        }
    }

    private void StartElection(object? sender, ElapsedEventArgs? e)
    {

        State = ServerState.Candidate;
        ++Term;
        Respond("Election Request");
        Votes = new List<Vote>() { new Vote(1, true) };
        tallyVotes();
    }

    private void tallyVotes()
    {
        if (positiveVotes() > NumServers / 2)
        {
            State = ServerState.Leader;
            sendHeartBeet(null, null);
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
        if (term >= Term)
        {
            LeaderId = senderID;
            State = ServerState.Follower;
            Respond("AppendReceived");
            ElectionTimer.Stop();
            ElectionTimer.Start();
            OtherServers.FirstOrDefault(s => s.ID == senderID)?.Confirm(term,ID);
        }
        else
        {
            Respond($"Leader is {LeaderId}");
        }
    }

    public void ReciveVote(int senderID, bool positveVote)
    {
        Votes.Add(new Vote(senderID, positveVote));
        tallyVotes();
    }

    public void RequestVote(int requesterId, int term)
    {
        int termVotedId = TermVotes.FirstOrDefault(t => t.Term == term)?.RequesterId ?? 0;

        if (termVotedId == requesterId)
        {
            Respond("Positive Vote");
        }
        else if (termVotedId != requesterId && termVotedId != 0)
        {
            Respond("Rejected Vote");
        }
        else // no votes for that term yet
        {
            TermVotes.Add(new TermVote(requesterId, term));
            Respond("Positive Vote");
        }
    }

    public void Confirm(int term, int reciverId)
    {
        throw new NotImplementedException();
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
