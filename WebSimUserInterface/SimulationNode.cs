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
    public int ElectionTimer { get => ((IServerAaron)InnerNode).ElectionTimer; set => ((IServerAaron)InnerNode).ElectionTimer = value; }
    public bool IsLive { get => ((IServerAaron)InnerNode).IsLive; set => ((IServerAaron)InnerNode).IsLive = value; }
    public int LeaderId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void AppendEntries()
    {
        InnerNode.AppendEntries();
    }

    public void AppendEntries(int senderID, string entry, int term)
    {
        throw new NotImplementedException();
    }

    public void Kill()
    {
        InnerNode.Kill();
    }
}
