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
    public bool IsLive { get => ((IServerAaron)InnerNode).IsLive; set => ((IServerAaron)InnerNode).IsLive = value; }
    public int LeaderId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public System.Timers.Timer ElectionTimer { get => ((IServerAaron)InnerNode).ElectionTimer; set => ((IServerAaron)InnerNode).ElectionTimer = value; }
    public int ID { get => ((IServerAaron)InnerNode).ID; set => ((IServerAaron)InnerNode).ID = value; }
    public int Term { get => ((IServerAaron)InnerNode).Term; set => ((IServerAaron)InnerNode).Term = value; }
    public List<TermVote> TermVotes { get => ((IServerAaron)InnerNode).TermVotes; set => ((IServerAaron)InnerNode).TermVotes = value; }

    public void AppendEntries(int senderID, string entry, int term)
    {
        ((IServerAaron)InnerNode).AppendEntries(senderID, entry, term);
    }

    public void Kill()
    {
        InnerNode.Kill();
    }

    public void RequestVote(int requesterId, int term)
    {
        ((IServerAaron)InnerNode).RequestVote(requesterId, term);
    }
}
