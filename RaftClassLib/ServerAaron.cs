
namespace RaftClassLib;

using Castle.Components.DictionaryAdapter.Xml;
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
    public List<LogEntry> Log { get; set; }
    public int commitIndex { get; set; } = -1;
    public List<int> nextIndexes { get; set; } = new List<int>();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor
    public ServerAaron(int id)
#pragma warning restore CS8618
	{
		this.ID = id;
		this.Term = 1;
		ElectionTimeoutMultiplier = 1;
		NetworkDelayModifier = 0;
		TermVotes = new List<TermVote>();
        Votes = new List<Vote>();
		State = ServerState.Follower;
		Sentmessages = new List<string>();
		OtherServers = new List<IServerAaron>();
        Log = new List<LogEntry>();
	}

	public async Task StartSimAsync()
	{
		startTimers();
		IsLive = true;
        await Task.CompletedTask;
	}

	private void startTimers()
    {
		int interval = Random.Shared.Next(150 * ElectionTimeoutMultiplier, 300 * ElectionTimeoutMultiplier);
		ElectionTimer = new Timer(interval);
		ElectionTimer.Elapsed += async (sender, e) => await StartElection(sender,e);
		ElectionTimer.AutoReset = true;
		ElectionTimer.Start();
		HBTimer = new Timer(50);
        HBTimer.Elapsed += async (sender, e) => await sendHeartBeet(sender, e);
		HBTimer.AutoReset = true;
        HBTimer.Start();
    }
    private void resetElectionTimer()
    {
		ElectionTimer.Stop();
		int interval = Random.Shared.Next(150 * ElectionTimeoutMultiplier, 300 * ElectionTimeoutMultiplier);
        ElectionTimer.Interval = interval;
        ElectionTimer.Start();
    }

    private async Task sendHeartBeet(object? sender, ElapsedEventArgs? e)
    {
        if(State == ServerState.Leader)
        {
            SelfLog("HB", Operation.None, -1);
            //List<LogEntry> newEntries = new List<LogEntry>();
            //for (int i = 0; i < Log.Count; i++)
            //{
            //    if (i > commitIndex)
            //    {
            //        newEntries.Add(Log[i]);
            //    }
            //}
            foreach (var server in OtherServers)
            {
				int indexInIndexes = getServerPositionInNextIndexes(server.ID);
				List<LogEntry> newEntries = new List<LogEntry>();
				for (int i = nextIndexes[indexInIndexes]; i < Log.Count; i++)
				{
					newEntries.Add(Log[i]);
				}
				AppendEntry ent = new AppendEntry(ID, "HB", Term, Operation.Default, commitIndex, newEntries, Log.Count);
                await server.AppendEntriesAsync(ent);
            }
        }
    }

    private async Task StartElection(object? sender, ElapsedEventArgs? e)
    {
        if (State == ServerState.Leader)
            return;
        resetElectionTimer();
		State = ServerState.Candidate;
        ++Term;
        SelfLog("Election Request", Operation.None, -1);
        Votes = new List<Vote>() { new Vote(ID, true) };
        await tallyVotesAsync();
        foreach (IServerAaron node in OtherServers)
        {
            await node.RequestVoteAsync(ID, Term);
        }

    }

    private async Task tallyVotesAsync()
    {
        if (positiveVotes() > NumServers / 2)
        {
            State = ServerState.Leader;
            nextIndexes = new List<int>();
            foreach( IServerAaron node in OtherServers)
            {
                nextIndexes.Add(0);
                await node.AppendEntriesAsync(new AppendEntry(ID, "REQUEST COMMIT INDEX", Term, Operation.None, commitIndex, new List<LogEntry>(), 0));
            }
            await sendHeartBeet(null, null);
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
    private void SelfLog(string message, Operation com, int term)
    {
        Sentmessages.Add(message);
        if (com is not Operation.None && term != -1)
            Log.Add(new LogEntry(term, com, message));
    }
    public async Task StopAsync()
    {
        IsLive = false;
        HBTimer.Stop();
        ElectionTimer.Stop();
        await Task.CompletedTask;
    }

    public async Task AppendEntriesAsync(AppendEntry Entry)
    {
        if (!IsLive) { return; }
        await PosibleDelay();
        IServerAaron sender = OtherServers.FirstOrDefault(s => s.ID == Entry.senderID) ?? new ServerAaron(-1);
        if (Entry.entry == "HB" && Entry.term >= Term)
		{
			await respondToHeartBeet(Entry, sender);
		}
		if (Entry.entry == "REQUEST COMMIT INDEX" && State == ServerState.Follower)
        {
            var message = new AppendEntry(ID, "COMMIT INDEX RESPONCE", Term, Operation.None, commitIndex, new List<LogEntry>() , Log.Count);
            await sender.AppendEntriesAsync(message);
        }
        else if (Entry.entry == "COMMIT INDEX RESPONCE" && State == ServerState.Leader)
		{
			int indexInIndexes = getServerPositionInNextIndexes(Entry.senderID);
			nextIndexes[indexInIndexes] = Entry.nextIndex;
		}
		else if (Entry.term >= Term)
		{
			await acceptNormalAppendEntrie(Entry, sender);
		}
		else
        {
            SelfLog($"Leader is {LeaderId}", Entry.command, Entry.term);
		}
    }

	private int getServerPositionInNextIndexes(int senderID)
	{
		var indexInIndexes = 0;
		while (indexInIndexes < OtherServers.Count)
		{
			if (OtherServers[indexInIndexes].ID == senderID) { break; }
			indexInIndexes++;
		}

		return indexInIndexes;
	}

	private async Task acceptNormalAppendEntrie(AppendEntry Entry, IServerAaron sender)
	{
		LeaderId = Entry.senderID;
		State = ServerState.Follower;
		SelfLog("AppendReceived", Entry.command, Entry.term);
		resetElectionTimer();
		if (sender.ID != -1)
		{ await sender.ConfirmAsync(Entry.term, ID); }
	}

	private async Task respondToHeartBeet(AppendEntry Entry, IServerAaron sender)
	{
		LeaderId = Entry.senderID;
		resetElectionTimer();
		if (sender.ID != -1)
			await sender.HBRecivedAsync(ID);
		State = ServerState.Follower;
	}

	private async Task PosibleDelay()
    {
        if (NetworkDelayModifier != 0)
        {
            await Task.Delay(NetworkDelayModifier);
        }
    }

    public async Task ReciveVoteAsync(int senderID, bool positveVote)
    {
        if (!IsLive) { return; }
        Votes.Add(new Vote(senderID, positveVote));
        if (State != ServerState.Leader)
        {
            await tallyVotesAsync();
        }
        await Task.CompletedTask;
    }

    public async Task RequestVoteAsync(int requesterId, int term)
    {
        if (!IsLive) { return; }
        int termVotedId = TermVotes.FirstOrDefault(t => t.Term == term)?.RequesterId ?? 0;
		IServerAaron sender = OtherServers.FirstOrDefault(s => s.ID == requesterId) ?? new ServerAaron(-1);
		if (termVotedId == requesterId) //Repeted Vote
        {
            SelfLog("Positive Vote", Operation.None, -1);
        }
        else if (termVotedId != requesterId && termVotedId != 0)
        {
            SelfLog("Rejected Vote", Operation.None, -1);
            if (sender.ID != -1)
                await sender.ReciveVoteAsync(ID, false);
        }
        else // no votes for that term yet
        {
            TermVotes.Add(new TermVote(requesterId, term));
			if (sender.ID != -1)
				await sender.ReciveVoteAsync(ID, true);
            SelfLog("Positive Vote", Operation.None, -1);
			ElectionTimer.Stop();
			resetElectionTimer();
		}
    }

    public async Task ConfirmAsync(int term, int reciverId)
    {
        await PosibleDelay();
        await Task.CompletedTask;
            //throw new NotImplementedException();
    }

    public async Task HBRecivedAsync(int reciverId)
    {
        await PosibleDelay();
        await Task.CompletedTask;
    }

	public async Task ClientRequestAsync(string value)
	{
        if (!IsLive) { return; }
        if (State == ServerState.Leader)
        {
            Log.Add(new LogEntry(Term, Operation.Default, value));
        }
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
