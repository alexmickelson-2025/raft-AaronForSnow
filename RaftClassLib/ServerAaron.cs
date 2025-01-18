
namespace RaftClassLib;

using System;
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
    public int ElectionTimeoutMultiplier { get; set; }
    public int NetworkDelayModifier { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    public ServerAaron(int id)
#pragma warning restore CS8618
	{
		this.ID = id;
		this.Term = 1;
		ElectionTimeoutMultiplier = 1;
		NetworkDelayModifier = 0;
		TermVotes = new List<TermVote>();
		State = ServerState.Follower;
		Sentmessages = new List<string>();
		OtherServers = new List<IServerAaron>();
	}

	public void StartSim()
	{
		startTimers();
		IsLive = true;
	}

	private void startTimers()
    {
        startElectiontimer();
        HBTimer = new Timer(50);
        HBTimer.Elapsed += sendHeartBeet;
        HBTimer.AutoReset = true;
        HBTimer.Start();
    }
    private void startElectiontimer()
    {
        Random random = new Random();
        int interval = random.Next(150 * ElectionTimeoutMultiplier, 300 * ElectionTimeoutMultiplier);
        ElectionTimer = new Timer(interval);
        ElectionTimer.Elapsed += StartElection;
        ElectionTimer.AutoReset = true;
        ElectionTimer.Start();
    }

    private void sendHeartBeet(object? sender, ElapsedEventArgs? e)
    {
        if(State == ServerState.Leader)
        {
            SelfLog("HB");
            foreach (var server in OtherServers)
            {
                server.AppendEntries(ID,"HB",Term);
            }
        }
    }

    private void StartElection(object? sender, ElapsedEventArgs? e)
    {

        State = ServerState.Candidate;
        ++Term;
        SelfLog("Election Request");
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
    private void SelfLog(string message)
    {
        Sentmessages.Add(message);
    }
    public void Kill()
    {
        IsLive = false;
    }

    public void AppendEntries(int senderID, string entry, int term)
    {
        PosibleDelay();
        if (entry == "HB")
        {
			ElectionTimer.Stop();
			startElectiontimer();
			OtherServers.FirstOrDefault(s => s.ID == senderID)?.HBRecived(ID);
        }
        else if (term >= Term)
        {
            LeaderId = senderID;
            State = ServerState.Follower;
            SelfLog("AppendReceived");
            ElectionTimer.Stop();
            startElectiontimer();
            OtherServers.FirstOrDefault(s => s.ID == senderID)?.Confirm(term, ID);
        }
        else
        {
            SelfLog($"Leader is {LeaderId}");
        }
    }

    private void PosibleDelay()
    {
        if (NetworkDelayModifier != 0)
        {
            Thread.Sleep(NetworkDelayModifier);
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

        if (termVotedId == requesterId) //Repeted Vote
        {
            SelfLog("Positive Vote");
        }
        else if (termVotedId != requesterId && termVotedId != 0)
        {
            SelfLog("Rejected Vote");
            OtherServers.FirstOrDefault(s => s.ID == requesterId)?.ReciveVote(ID, false);

        }
        else // no votes for that term yet
        {
            TermVotes.Add(new TermVote(requesterId, term));
            OtherServers.FirstOrDefault(s => s.ID == requesterId)?.ReciveVote(ID, true);
            SelfLog("Positive Vote");
			ElectionTimer.Stop();
			startElectiontimer();
		}
    }

    public void Confirm(int term, int reciverId)
    {
        PosibleDelay();
            //throw new NotImplementedException();
    }

    public void HBRecived(int reciverId)
    {
        PosibleDelay();
        //throw new NotImplementedException();
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
