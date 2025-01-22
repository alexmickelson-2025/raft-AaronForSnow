﻿
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
    public List<Log> Logs { get; set; }

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
        Logs = new List<Log>();
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
		ElectionTimer.Stop();
		int interval = Random.Shared.Next(150 * ElectionTimeoutMultiplier, 300 * ElectionTimeoutMultiplier);
        ElectionTimer.Interval = interval;
        ElectionTimer.Start();
    }

    private void sendHeartBeet(object? sender, ElapsedEventArgs? e)
    {
        if(State == ServerState.Leader)
        {
            SelfLog("HB", Operation.None, -1);
            foreach (var server in OtherServers)
            {
                server.AppendEntries(ID,"HB",Term);
            }
        }
    }

    private void StartElection(object? sender, ElapsedEventArgs? e)
    {
        resetElectionTimer();
		State = ServerState.Candidate;
        ++Term;
        SelfLog("Election Request", Operation.None, -1);
        Votes = new List<Vote>() { new Vote(ID, true) };
        tallyVotes();
        foreach (IServerAaron node in OtherServers)
        {
            node.RequestVote(ID, Term);
        }

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
    private void SelfLog(string message, Operation com, int term)
    {
        Sentmessages.Add(message);
        if (com is not Operation.None && term != -1)
            Logs.Add(new Log() { Command = com, Term = term });
    }
    public async Task Kill()
    {
        IsLive = false;
        await Task.CompletedTask;
    }

    public async Task AppendEntries(int senderID, string entry, int term, Operation? command = Operation.None, int? index = -1)
    {
        await PosibleDelay();
        if (entry == "HB")
        {
			
			resetElectionTimer();
			await OtherServers.FirstOrDefault(s => s.ID == senderID)?.HBRecived(ID);
            State = ServerState.Follower;
        }
        else if (term >= Term)
        {
            LeaderId = senderID;
            State = ServerState.Follower;
            SelfLog("AppendReceived", command ?? Operation.None, term);
            ElectionTimer.Stop();
            resetElectionTimer();
            await OtherServers.FirstOrDefault(s => s.ID == senderID)?.Confirm(term, ID);
        }
        else
        {
            SelfLog($"Leader is {LeaderId}", command ?? Operation.None, term);
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
            SelfLog("Positive Vote", Operation.None, -1);
        }
        else if (termVotedId != requesterId && termVotedId != 0)
        {
            SelfLog("Rejected Vote", Operation.None, -1);
            await OtherServers.FirstOrDefault(s => s.ID == requesterId)?.ReciveVote(ID, false);

        }
        else // no votes for that term yet
        {
            TermVotes.Add(new TermVote(requesterId, term));
            await OtherServers.FirstOrDefault(s => s.ID == requesterId)?.ReciveVote(ID, true);
            SelfLog("Positive Vote", Operation.None, -1);
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
