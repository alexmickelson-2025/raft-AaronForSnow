namespace RaftClassLib
{
	using System.Runtime.CompilerServices;
	using System.Timers;
    public interface IServerAaron
    {
        public ServerState State {get; set;}
        public string StateMachineDataBucket {get;}  
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
        Task AppendEntriesAsync(AppendEntry Entry);
        Task StopAsync();
        Task RequestVoteAsync(int requesterId, int term);
        Task ConfirmAsync(int term, int reciverId, int indexOfLog = 0);
        Task HBRecivedAsync(int reciverId);
        Task ReciveVoteAsync(int senderID, bool v);
        Task StartSimAsync();
        Task ClientRequestAsync(string value);


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