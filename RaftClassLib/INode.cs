using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftClassLib;

public interface INode
{
    Task AppendEntriesAsync(AppendEntry Entry);
    Task StartSimAsync();
    Task StopAsync();
    Task RequestVoteAsync(RequestVoteDTO request);
    Task ReciveVoteAsync(ReceiveVoteDTO ballet);
    Task ConfirmAsync(ConfirmationDTO info);
    Task HBRecivedAsync(int reciverId);
}
public record RequestVoteDTO
{
    public RequestVoteDTO(int requsterId, int term)
    {
        RequesterId = requsterId;
        Term = term;
    }
    public int RequesterId { get; }
    public int Term { get; }
}
public record ReceiveVoteDTO
{
    public ReceiveVoteDTO(int senderId, bool isPositiveVote)
    {
        SenderID = senderId;
        IsPositiveVote = isPositiveVote;
    }
    public int SenderID { get; }
    public bool IsPositiveVote { get; }
}
public record ConfirmationDTO
{
    public ConfirmationDTO(int term, int reciverId, int indexOfLog = 0)
    {
        Term = term;
        ReciverId = reciverId;
        IndexOfLog = indexOfLog;
    }
    public int Term { get;}
    public int ReciverId { get; }
    public int IndexOfLog { get; }
}