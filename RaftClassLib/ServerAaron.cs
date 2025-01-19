
namespace RaftClassLib;

using System;
using System.Timers;
public class ServerAaron : IServerAaron
{
    public ServerState State { get; set; }
    public System.Timers.Timer ElectionTimer { get; set; }
    public System.Timers.Timer HBTimer { get; set; }
    public bool IsLive { get; set; }
    public int LeaderId { get; set; }
    public int ID {  get; set; }
    public int Term { get; set; }
    private int NumServers { get => OtherServers.Count + 1; }
    public int ElectionTimeoutMultiplier { get; set; }
    public int NetworkDelayModifier { get; set; }
    public List<string> Sentmessages { get; set; }
    public List<Vote> Votes { get; set; }
    public List<TermVote> TermVotes { get; set; }
    public List<IServerAaron> OtherServers { get; set; }

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

	public async Task StartSim()
	{
		startTimers();
		IsLive = true;
        await Task.CompletedTask;
	}

	private void startTimers()
    {
		int interval = Random.Shared.Next(150 * ElectionTimeoutMultiplier, 300 * ElectionTimeoutMultiplier);
		ElectionTimer = new Timer(interval);
		ElectionTimer.Elapsed += StartElection;
		ElectionTimer.AutoReset = true;
		ElectionTimer.Start();
		HBTimer = new Timer(50);
        HBTimer.Elapsed += sendHeartBeet;
        HBTimer.AutoReset = true;
        HBTimer.Start();
    }
    private void resetElectionTimer()
    {
        int interval = Random.Shared.Next(150 * ElectionTimeoutMultiplier, 300 * ElectionTimeoutMultiplier);
        ElectionTimer.Interval = interval;
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
    public async Task Kill()
    {
        IsLive = false;
        await Task.CompletedTask;
    }

    public async Task AppendEntries(int senderID, string entry, int term)
    {
        await PosibleDelay();
        if (entry == "HB")
        {
			ElectionTimer.Stop();
			resetElectionTimer();
			await OtherServers.FirstOrDefault(s => s.ID == senderID)?.HBRecived(ID);
            State = ServerState.Follower;
        }
        else if (term >= Term)
        {
            LeaderId = senderID;
            State = ServerState.Follower;
            SelfLog("AppendReceived");
            ElectionTimer.Stop();
            resetElectionTimer();
            await OtherServers.FirstOrDefault(s => s.ID == senderID)?.Confirm(term, ID);
        }
        else
        {
            SelfLog($"Leader is {LeaderId}");
        }
    }

    private async Task PosibleDelay()
    {
        if (NetworkDelayModifier != 0)
        {
            await Task.Delay(NetworkDelayModifier);
        }
    }

    public async Task ReciveVote(int senderID, bool positveVote)
    {
        Votes.Add(new Vote(senderID, positveVote));
        tallyVotes();
        await Task.CompletedTask;
    }

    public async Task RequestVote(int requesterId, int term)
    {
        int termVotedId = TermVotes.FirstOrDefault(t => t.Term == term)?.RequesterId ?? 0;

        if (termVotedId == requesterId) //Repeted Vote
        {
            SelfLog("Positive Vote");
        }
        else if (termVotedId != requesterId && termVotedId != 0)
        {
            SelfLog("Rejected Vote");
            await OtherServers.FirstOrDefault(s => s.ID == requesterId)?.ReciveVote(ID, false);

        }
        else // no votes for that term yet
        {
            TermVotes.Add(new TermVote(requesterId, term));
            await OtherServers.FirstOrDefault(s => s.ID == requesterId)?.ReciveVote(ID, true);
            SelfLog("Positive Vote");
			ElectionTimer.Stop();
			resetElectionTimer();
		}
    }

    public async Task Confirm(int term, int reciverId)
    {
        await PosibleDelay();
        await Task.CompletedTask;
            //throw new NotImplementedException();
    }

    public async Task HBRecived(int reciverId)
    {
        await PosibleDelay();
        await Task.CompletedTask;
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
