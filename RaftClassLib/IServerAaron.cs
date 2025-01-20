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
        public List<Vote> Votes { get; set; }
        public List<TermVote> TermVotes { get; set; }
        public List<IServerAaron> OtherServers { get; set; }
        public List<Log> Logs { get; set; }
        Task AppendEntries(int senderID, string entry, int term, Operation? command = Operation.None, int? index = -1);
        Task Kill();
        Task RequestVote(int requesterId, int term);
        Task Confirm(int term, int reciverId);
        Task HBRecived(int reciverId);
        Task ReciveVote(int senderID, bool v);
        Task StartSim();


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
    public class Log
    {
        public int Term { get; set; }
        public Operation Command { get; set; }
	}
    public enum Operation
    {
        Default,
        None
    }
}