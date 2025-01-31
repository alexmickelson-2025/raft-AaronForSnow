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
