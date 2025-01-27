namespace RaftClassLib
{
	using System.Runtime.CompilerServices;
	using System.Timers;
    public interface IServerAaron
    {
        public ServerState State {get; set;}
        public List<string> Sentmessages { get; set;}   
        public Timer ElectionTimer { get; set;}
        public bool IsLive { get; set;}
        public int LeaderId { get; set;}
        public int ID { get; set; }
        public int Term { get; set; }
        public int ElectionTimeoutMultiplier { get; set; }
        public int NetworkDelayModifier { get; set; }
        public int commitIndex { get; set; }
        public List<int> nextIndexes { get; set; }
        public List<Vote> Votes { get; set; }
        public List<TermVote> TermVotes { get; set; }
        public List<IServerAaron> OtherServers { get; set; }
        public List<LogEntry> Log { get; set; }
        Task AppendEntries(AppendEntry Entry);
        Task Kill();
        Task RequestVote(int requesterId, int term);
        Task Confirm(int term, int reciverId);
        Task HBRecived(int reciverId);
        Task ReciveVote(int senderID, bool v);
        Task StartSim();
        Task ClientRequest(string value);


	}
    public class TermVote
    {
        public readonly int RequesterId;
        public readonly int Term;
        public TermVote(int requesterId, int term)
        {
            RequesterId = requesterId;
            Term = term;
        }
    }
    
    public enum Operation
    {
        Default,
        None
    }
}