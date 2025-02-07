namespace RaftClassLib;
public class NodeData
{
	public int Id {get; set;}
	public bool Status {get; set;}
	public double ElectionTimeout {get; set;}
	public int Term {get; set;}
	public int CurrentTermLeader {get; set;}
	public int CommittedEntryIndex {get; set;}
	public List<LogEntry> Log {get; set;}
	public ServerState State {get; set;}
	public int NodeIntervalScalar {get; set;}

	public NodeData(int Id, bool Status, double ElectionTimeout, int Term, int CurrentTermLeader, int CommittedEntryIndex, List<LogEntry> Log, ServerState State, int NodeIntervalScalar)
	{
		this.Id = Id;
		this.Status = Status;
		this.ElectionTimeout = ElectionTimeout;
		this.Term = Term;
		this.CurrentTermLeader = CurrentTermLeader;
		this.CommittedEntryIndex = CommittedEntryIndex;
		this.Log = Log;
		this.State = State;
		this.NodeIntervalScalar = NodeIntervalScalar;
	}
}