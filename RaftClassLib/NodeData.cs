namespace RaftClassLib;
public class NodeData
{
	public int id {get; set;}
	public bool status {get; set;}
	public double electionTimeout {get; set;}
	public int term {get; set;}
	public int currentTermLeader {get; set;}
	public int committedEntryIndex {get; set;}
	public List<LogEntry> log {get; set;}
	public ServerState state {get; set;}
	public int nodeIntervalScalar {get; set;}
	// public NodeData()
	// {
	// 	id = 0;
	// 	status = false;
	// 	electionTimeout = 0;
	// 	term  = 0;
	// 	currentTermLeader = 0;
	// 	committedEntryIndex = 0;
	// 	log = new List<LogEntry>();
	// 	state = ServerState.Follower;
	// 	nodeIntervalScalar = 0;
	// }

	public NodeData(int Id, bool Status, double ElectionTimeout, int Term, int CurrentTermLeader, int CommittedEntryIndex, List<LogEntry> Log, ServerState State, int NodeIntervalScalar)
	{
		id = Id;
		status = Status;
		electionTimeout = ElectionTimeout;
		term = Term;
		currentTermLeader = CurrentTermLeader;
		committedEntryIndex = CommittedEntryIndex;
		log = Log;
		state = State;
		nodeIntervalScalar = NodeIntervalScalar;
	}
}