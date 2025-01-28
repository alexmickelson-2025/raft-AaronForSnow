using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftClassLib;

public class AppendEntryOld
{
	public int senderID { get; set; }
	public string entry { get; set; } = "";
	public int term { get; set; }
	public Operation command { get; set; } = Operation.None;
	public int commitedIndex { get; set; }
	public List<LogEntry> newLogs { get; set; } = new List<LogEntry>();
}
public record AppendEntry (
	int senderID,
	string entry, 
	int term,
	Operation command,
	int commitedIndex,
	List<LogEntry> newLogs,
	int nextIndex = 0
	);
