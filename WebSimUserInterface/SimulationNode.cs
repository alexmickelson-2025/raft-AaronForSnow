using RaftClassLib;

namespace WebSimUserInterface;

public class SimulationNode : IServerAaron
{
    public readonly ServerAaron InnerNode;
    public SimulationNode(ServerAaron innerNode)
    {
        this.InnerNode = innerNode;
    }

	public ServerState State { get => InnerNode.State; set => InnerNode.State = value; }
	public System.Timers.Timer ElectionTimer { get => InnerNode.ElectionTimer; set => InnerNode.ElectionTimer = value; }
	public bool IsLive { get => InnerNode.IsLive; set => InnerNode.IsLive = value; }
	public int LeaderId { get => InnerNode.LeaderId; set => InnerNode.LeaderId = value; }
	public int ID { get => InnerNode.ID; set => InnerNode.ID = value; }
	public int Term { get => InnerNode.Term; set => InnerNode.Term = value; }
	public int ElectionTimeoutMultiplier { get => InnerNode.ElectionTimeoutMultiplier; set => InnerNode.ElectionTimeoutMultiplier = value; }
	public int NetworkDelayModifier { get => InnerNode.NetworkDelayModifier; set => InnerNode.NetworkDelayModifier = value; }
	public List<LogEntry> Log { get => InnerNode.Log; set => InnerNode.Log = value; }
	public int commitIndex { get => InnerNode.commitIndex; set => InnerNode.commitIndex = value; }
	public List<int> nextIndexes { get => InnerNode.nextIndexes; set => InnerNode.nextIndexes = value; }

	public string StateMachineDataBucket => InnerNode.StateMachineDataBucket;

	public Task AppendEntriesAsync(AppendEntry Entry)
	{
		return ((IServerAaron)InnerNode).AppendEntriesAsync(Entry);
	}

    public Task ClientRequestAsync(string value)
    {
        return ((IServerAaron)InnerNode).ClientRequestAsync(value);
    }

	public Task HBReceivedAsync(int reciverId)
	{
		return ((IServerAaron)InnerNode).HBReceivedAsync(reciverId);
	}

	public Task StopAsync()
	{
		return ((IServerAaron)InnerNode).StopAsync();
	}

	public Task StartSimAsync()
	{
		return ((IServerAaron)InnerNode).StartSimAsync();
	}

	public Task RequestVoteAsync(RequestVoteDTO request)
	{
		return ((IServerAaron)InnerNode).RequestVoteAsync(request);
	}

	public Task ConfirmAsync(ConfirmationDTO confirm)
	{
		return ((IServerAaron)InnerNode).ConfirmAsync(confirm);
	}

	public Task ReceiveVoteAsync(ReceiveVoteDTO vote)
	{
		return ((IServerAaron)InnerNode).ReceiveVoteAsync(vote);
	}
}
