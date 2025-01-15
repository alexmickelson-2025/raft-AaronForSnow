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

    public void AppendEntries()
    {
        InnerNode.AppendEntries();
    }

    public void Kill()
    {
        InnerNode.Kill();
    }
}
