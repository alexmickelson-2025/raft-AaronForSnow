using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftClassLib;

public class AppendEntry
{
	public int senderID { get; set; }
	public string entry { get; set; } = "";
	public int term { get; set; }
	public Operation command { get; set; } = Operation.None;
	public int index { get; set; }
}
