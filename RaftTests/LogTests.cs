using Castle.Components.DictionaryAdapter.Xml;
using NSubstitute;
using RaftClassLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftTests;

public class LogTests
{
//	Given a log entry
//Then the log entry contains a state machine command
//And the log entry contains the the term number at which the leader received it
//And the log entry contains an integer index identifying its position in the log
	[Fact]
	public void LogEntryContainsComponents()
	{
		IServerAaron testServer = new ServerAaron(1);
		testServer.AppendEntries(
			new AppendEntry() {
			senderID = 1,
			entry = "test Log",
			term = 2,
			command = Operation.Default,
			index = 0
			});
		Assert.Single(testServer.Logs);
		Assert.Equal(Operation.Default, testServer.Logs[0].Command);
		Assert.Equal(2, testServer.Logs[0].Term);
	}
}
