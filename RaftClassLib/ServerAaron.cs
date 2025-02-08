
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
    public List<Vote> Votes { get; set; }
    public List<TermVote> TermVotes { get; set; }
    public List<IServerAaron> OtherServers { get; set; }
    public List<LogEntry> Log { get; set; }
    public int commitIndex { get; set; } = -1;
    public List<int> nextIndexes { get; set; } = new List<int>();
    private List<Confirmaiton> confirmesRecived { get; set; }
	public string StateMachineDataBucket { get; private set; }

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
		OtherServers = new List<IServerAaron>();
        Log = new List<LogEntry>();
        confirmesRecived = new List<Confirmaiton>();
        StateMachineDataBucket = "";
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
		HBTimer = new Timer(50 * ElectionTimeoutMultiplier);
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
            foreach (var server in OtherServers)
            {
				int indexInIndexes = getServerPositionInNextIndexes(server.ID);
				List<LogEntry> newEntries = new List<LogEntry>();
                AppendEntry ent;
				for (int i = nextIndexes[indexInIndexes]; i < Log.Count; i++)
				{
					newEntries.Add(Log[i]);
				}
				ent = new AppendEntry(ID, "HB", Term, Operation.Default, commitIndex, newEntries, Log.Count);
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
        Votes = new List<Vote>() { new Vote(ID, true) };
        await tallyVotesAsync();
        foreach (IServerAaron node in OtherServers)
        {
            await node.RequestVoteAsync(new RequestVoteDTO(ID, Term));
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
        IServerAaron sender = OtherServers.FirstOrDefault(s => s.ID == Entry.SenderID) ?? new ServerAaron(-1);
        if (Entry.Entry == "HB" && Entry.Term >= Term && Entry.CommitedIndex >= commitIndex)
		{
			if (Entry.Term == Term || Entry.Term == Term +1 && Entry.NextIndex == Log.Count) // it should be a valid heart beat
            {
                foreach (LogEntry l in Entry.NewLogs)
                {
                    Log.Add(l);
					await sender.ConfirmAsync(new ConfirmationDTO(l.Term, ID, Log.Count - 1));
				}
                await CheckCommitedIndexAsync(Entry.CommitedIndex);
            }
			await respondToHeartBeet(sender);
		}
		if (Entry.Entry == "REQUEST COMMIT INDEX" && State == ServerState.Follower)
        {
            var message = new AppendEntry(ID, "COMMIT INDEX RESPONCE", Term, Operation.None, commitIndex, new List<LogEntry>() , Log.Count);
            await sender.AppendEntriesAsync(message);
        }
        else if (Entry.Entry == "COMMIT INDEX RESPONCE" && State == ServerState.Leader)
		{
			int indexInIndexes = getServerPositionInNextIndexes(Entry.SenderID);
			nextIndexes[indexInIndexes] = Entry.NextIndex;
		}
		else if (!Entry.Entry.Contains("Leader is"))
        {
			var message = new AppendEntry(ID, $"Leader is {LeaderId}", Term, Operation.None, commitIndex, new List<LogEntry>(), Log.Count);
			await sender.AppendEntriesAsync(message);
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

	private async Task respondToHeartBeet( IServerAaron sender)
	{
		LeaderId = sender.ID;
		resetElectionTimer();
		if (sender.ID != -1)
			await sender.HBReceivedAsync(ID);
		State = ServerState.Follower;
	}

	private async Task PosibleDelay()
    {
        if (NetworkDelayModifier != 0)
        {
            await Task.Delay(NetworkDelayModifier);
        }
    }
    public async Task ReceiveVoteAsync(ReceiveVoteDTO vote)
    {
        await ReciveVoteAsync(vote.SenderID, vote.IsPositiveVote);
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
    public async Task RequestVoteAsync(RequestVoteDTO vote)
    {
        await RequestVoteAsync(vote.RequesterId, vote.Term);
    }
	public async Task RequestVoteAsync(int requesterId, int term)
    {
        if (!IsLive) { return; }
        int termVotedId = TermVotes.FirstOrDefault(t => t.Term == term)?.RequesterId ?? 0;
		IServerAaron sender = OtherServers.FirstOrDefault(s => s.ID == requesterId) ?? new ServerAaron(-1);
		if (termVotedId == requesterId) //Repeted Vote
        {
			await sender.ReceiveVoteAsync(new ReceiveVoteDTO(ID, true));
        }
        else if (termVotedId != requesterId && termVotedId != 0)
        {
            if (sender.ID != -1)
                await sender.ReceiveVoteAsync(new ReceiveVoteDTO(ID, false));
        }
        else // no votes for that term yet
        {
            TermVotes.Add(new TermVote(requesterId, term));
			if (sender.ID != -1)
				await sender.ReceiveVoteAsync(new ReceiveVoteDTO(ID, true));
			ElectionTimer.Stop();
			resetElectionTimer();
		}
    }
    public async Task ConfirmAsync(ConfirmationDTO confirm)
	{
        await ConfirmAsync(confirm.Term, confirm.ReciverId, confirm.IndexOfLog);
    }
	public async Task ConfirmAsync(int term, int reciverId, int indexOfLog = 0)
    {
        await PosibleDelay();
        Confirmaiton con = new Confirmaiton(term, reciverId, indexOfLog);
        if (State == ServerState.Leader && indexOfLog < Log.Count && indexOfLog > commitIndex)
		{
			if (!confirmesRecived.Contains(con))
			{
				confirmesRecived.Add(con);
			}
			int count = getNumConfirmedForIndex(indexOfLog);
			if (count >= OtherServers.Count / 2)
			{
				await OtherServers.Where(s => s.ID == reciverId).First().ConfirmAsync(new ConfirmationDTO(term, ID, indexOfLog));
				await CheckCommitedIndexAsync();
			}
		}
	}

	private int getNumConfirmedForIndex(int indexOfLog)
	{
		return confirmesRecived.Where(c => c.indexOfLog == indexOfLog).Count();
	}

	private async Task CheckCommitedIndexAsync(int expectedCommitIndex = -1)
    {
        switch (State)
        {
            case ServerState.Leader:
				int count = getNumConfirmedForIndex(commitIndex + 1);
				if (count >= OtherServers.Count / 2 && Log.Count > commitIndex + 1)
				{
					++commitIndex;
					StateMachineDataBucket += Log[commitIndex].UniqueValue;
					await CheckCommitedIndexAsync();
                    
				}
                break;
            case ServerState.Follower:
                if (commitIndex < expectedCommitIndex && Log.Count > commitIndex + 1)
                {
					++commitIndex;
                    StateMachineDataBucket += Log[commitIndex].UniqueValue;
                    await CheckCommitedIndexAsync(expectedCommitIndex);
				}
                break;
		}
    }


	public async Task HBReceivedAsync(int ReceiverId)
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
public record Confirmaiton(int term, int reciverId, int indexOfLog = 0);
