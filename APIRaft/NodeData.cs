using RaftClassLib;

internal class NodeData
{
	private object id;
	private object status;
	private object electionTimeout;
	private object term;
	private object currentTermLeader;
	private object committedEntryIndex;
	private List<LogEntry> log;
	private ServerState state;
	private object nodeIntervalScalar;

	public NodeData(object Id, object Status, object ElectionTimeout, object Term, object CurrentTermLeader, object CommittedEntryIndex, List<LogEntry> Log, ServerState State, object NodeIntervalScalar)
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