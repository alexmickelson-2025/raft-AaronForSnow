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
    }
}