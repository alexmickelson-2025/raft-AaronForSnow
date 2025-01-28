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
	public async Task LogEntryContainsComponents()
	{
		IServerAaron testServer = new ServerAaron(1);
		await testServer.StartSimAsync();
		await testServer.AppendEntriesAsync(
			new AppendEntry(1, "test log", 2, Operation.Default, 0, new List<LogEntry>()));
		Assert.Single(testServer.Log);
		Assert.Equal(Operation.Default, testServer.Log[0].Command);
		Assert.Equal(2, testServer.Log[0].Term);
	}
}
