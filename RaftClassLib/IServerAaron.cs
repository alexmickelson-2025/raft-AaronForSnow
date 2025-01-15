namespace RaftClassLib
{
    public interface IServerAaron
    {
        void AppendEntries();
        void Kill();
        public ServerState State {get; set;}
        public List<string> Sentmessages { get; set;}   
        public int ElectionTimer { get; set;}
        public bool IsLive { get; set;}
    }
}