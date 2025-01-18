namespace RaftClassLib
{
    using System.Timers;
    public interface IServerAaron
    {
        void AppendEntries(int senderID, string entry, int term);
        void Kill();
        public ServerState State {get; set;}
        public List<string> Sentmessages { get; set;}   
        public Timer ElectionTimer { get; set;}
        public bool IsLive { get; set;}
        public int LeaderId { get; set;}
        public int ID { get; set; }
        public int Term { get; set; }
        void RequestVote(int requesterId, int term);
        void Confirm(int term, int reciverId);
        void ReciveVote(int senderID, bool v);

        public List<Vote> Votes { get; set; }
        public List<TermVote> TermVotes { get; set; }
        public List<IServerAaron> OtherServers { get; set; }
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