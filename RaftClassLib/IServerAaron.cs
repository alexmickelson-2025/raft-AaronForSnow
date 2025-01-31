namespace RaftClassLib
{
	using System.Runtime.CompilerServices;
	using System.Timers;
    public interface IServerAaron
    {
        public int ID { get; set; }
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