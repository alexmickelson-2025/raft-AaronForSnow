namespace RaftClassLib
{
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
        Task AppendEntries(int senderID, string entry, int term);
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
}