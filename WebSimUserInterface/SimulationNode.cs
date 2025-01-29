using RaftClassLib;

namespace WebSimUserInterface;

public class SimulationNode : IServerAaron
{
    public readonly ServerAaron InnerNode;
    public SimulationNode(ServerAaron innerNode)
    {
        this.InnerNode = innerNode;
    }

	public ServerState State { get => ((IServerAaron)InnerNode).State; set => ((IServerAaron)InnerNode).State = value; }
	public List<string> Sentmessages { get => ((IServerAaron)InnerNode).Sentmessages; set => ((IServerAaron)InnerNode).Sentmessages = value; }
	public System.Timers.Timer ElectionTimer { get => ((IServerAaron)InnerNode).ElectionTimer; set => ((IServerAaron)InnerNode).ElectionTimer = value; }
	public bool IsLive { get => ((IServerAaron)InnerNode).IsLive; set => ((IServerAaron)InnerNode).IsLive = value; }
	public int LeaderId { get => ((IServerAaron)InnerNode).LeaderId; set => ((IServerAaron)InnerNode).LeaderId = value; }
	public int ID { get => ((IServerAaron)InnerNode).ID; set => ((IServerAaron)InnerNode).ID = value; }
	public int Term { get => ((IServerAaron)InnerNode).Term; set => ((IServerAaron)InnerNode).Term = value; }
	public int ElectionTimeoutMultiplier { get => ((IServerAaron)InnerNode).ElectionTimeoutMultiplier; set => ((IServerAaron)InnerNode).ElectionTimeoutMultiplier = value; }
	public int NetworkDelayModifier { get => ((IServerAaron)InnerNode).NetworkDelayModifier; set => ((IServerAaron)InnerNode).NetworkDelayModifier = value; }
	public List<Vote> Votes { get => ((IServerAaron)InnerNode).Votes; set => ((IServerAaron)InnerNode).Votes = value; }
	public List<TermVote> TermVotes { get => ((IServerAaron)InnerNode).TermVotes; set => ((IServerAaron)InnerNode).TermVotes = value; }
	public List<IServerAaron> OtherServers { get => ((IServerAaron)InnerNode).OtherServers; set => ((IServerAaron)InnerNode).OtherServers = value; }
	public List<LogEntry> Log { get => ((IServerAaron)InnerNode).Log; set => ((IServerAaron)InnerNode).Log = value; }
	public int commitIndex { get => ((IServerAaron)InnerNode).commitIndex; set => ((IServerAaron)InnerNode).commitIndex = value; }
	public List<int> nextIndexes { get => ((IServerAaron)InnerNode).nextIndexes; set => ((IServerAaron)InnerNode).nextIndexes = value; }

	public string StateMachineDataBucket => ((IServerAaron)InnerNode).StateMachineDataBucket;

	public Task AppendEntriesAsync(AppendEntry Entry)
	{
		return ((IServerAaron)InnerNode).AppendEntriesAsync(Entry);
	}

    public Task ClientRequestAsync(string value)
    {
        return ((IServerAaron)InnerNode).ClientRequestAsync(value);
    }

	public Task HBRecivedAsync(int reciverId)
	{
		return ((IServerAaron)InnerNode).HBRecivedAsync(reciverId);
	}

	public Task StopAsync()
	{
		return ((IServerAaron)InnerNode).StopAsync();
	}

	public Task ReciveVoteAsync(int senderID, bool v)
	{
		return ((IServerAaron)InnerNode).ReciveVoteAsync(senderID, v);
	}

	public Task RequestVoteAsync(int requesterId, int term)
	{
		return ((IServerAaron)InnerNode).RequestVoteAsync(requesterId, term);
	}

	public Task StartSimAsync()
	{
		return ((IServerAaron)InnerNode).StartSimAsync();
	}

	public Task ConfirmAsync(int term, int reciverId, int indexOfLog = 0)
	{
		return ((IServerAaron)InnerNode).ConfirmAsync(term, reciverId, indexOfLog);
	}
}
