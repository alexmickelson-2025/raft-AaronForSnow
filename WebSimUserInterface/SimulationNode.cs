using RaftClassLib;

namespace WebSimUserInterface;

public class SimulationNode : IServerAaron
{
    public readonly ServerAaron InnerNode;
    public SimulationNode(ServerAaron innerNode)
    {
        this.InnerNode = innerNode;
        this.InnerNode.ElectionTimeoutMultiplier = 100;
    }

    public ServerState State { get => ((IServerAaron)InnerNode).State; set => ((IServerAaron)InnerNode).State = value; }
    public List<string> Sentmessages { get => ((IServerAaron)InnerNode).Sentmessages; set => ((IServerAaron)InnerNode).Sentmessages = value; }
    public bool IsLive { get => ((IServerAaron)InnerNode).IsLive; set => ((IServerAaron)InnerNode).IsLive = value; }
    public System.Timers.Timer ElectionTimer { get => ((IServerAaron)InnerNode).ElectionTimer; set => ((IServerAaron)InnerNode).ElectionTimer = value; }
    public int ID { get => ((IServerAaron)InnerNode).ID; set => ((IServerAaron)InnerNode).ID = value; }
    public int Term { get => ((IServerAaron)InnerNode).Term; set => ((IServerAaron)InnerNode).Term = value; }
    public List<TermVote> TermVotes { get => ((IServerAaron)InnerNode).TermVotes; set => ((IServerAaron)InnerNode).TermVotes = value; }

    public List<Vote> Votes { get => ((IServerAaron)InnerNode).Votes; set => ((IServerAaron)InnerNode).Votes = value; }
    public int LeaderId { get => ((IServerAaron)InnerNode).LeaderId; set => ((IServerAaron)InnerNode).LeaderId = value; }
    public List<IServerAaron> OtherServers { get => ((IServerAaron)InnerNode).OtherServers; set => ((IServerAaron)InnerNode).OtherServers = value; }
    public int ElectionTimeoutMultiplier { get => ((IServerAaron)InnerNode).ElectionTimeoutMultiplier; set => ((IServerAaron)InnerNode).ElectionTimeoutMultiplier = value; }
    public int NetworkDelayModifier { get => ((IServerAaron)InnerNode).NetworkDelayModifier; set => ((IServerAaron)InnerNode).NetworkDelayModifier = value; }

    public void AppendEntries(int senderID, string entry, int term)
    {
        ((IServerAaron)InnerNode).AppendEntries(senderID, entry, term);
    }

    public void Confirm(int term, int reciverId)
    {
        ((IServerAaron)InnerNode).Confirm(term, reciverId);
    }

    public void HBRecived(int reciverId)
    {
        ((IServerAaron)InnerNode).HBRecived(reciverId);
    }

    public void Kill()
    {
        InnerNode.Kill();
    }

    public void ReciveVote(int senderID, bool v)
    {
        ((IServerAaron)InnerNode).ReciveVote(senderID, v);
    }

    public void RequestVote(int requesterId, int term)
    {
        ((IServerAaron)InnerNode).RequestVote(requesterId, term);
    }

	public void StartSim()
	{
        InnerNode.StartSim();
	}
}
