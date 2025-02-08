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
	public async Task ClientRequestAddsToLeadersLogs()
	{
		ServerAaron testServer = new ServerAaron(1);
		testServer.State = ServerState.Leader;
		await testServer.StartSimAsync();
		Assert.Empty(testServer.Log);
		await testServer.ClientRequestAsync("test request from client");
		//await testServer.AppendEntriesAsync(
		//	new AppendEntry(1, "test log", 2, Operation.Default, 0, new List<LogEntry>()));
		Assert.Single(testServer.Log);
		Assert.Equal(Operation.Default, testServer.Log[0].Command);
		Assert.Equal(-1, testServer.commitIndex);// not commited
	}
	[Fact]
	public async Task LeadersForwardsClientRequestInNextHB()
	{
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		testServer.State = ServerState.Leader;
		await testServer.StartSimAsync();
		Assert.Empty(testServer.Log);
		await testServer.ClientRequestAsync("test request from client");
		Thread.Sleep(60);
		await fake1.Received().AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.SenderID == 3));
		await fake1.Received().AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.NewLogs.Count == 1));
		await fake1.Received().AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.NewLogs[0] == new LogEntry(1,Operation.Default, "test request from client")));
		await fake1.Received().AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.NextIndex == 1));
		await fake1.Received().AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.CommitedIndex == -1));
		await fake1.Received().AppendEntriesAsync(Arg.Is<AppendEntry>(e => e.Entry == "HB"));
	}
	[Fact]
	public async Task ClientRecivesRequestInNextHBAndRespondsToLeader()
	{
		IServerAaron fake1;
		ServerAaron testServer;
		Tools.SetUpThreeServers(out fake1, out testServer);
		await testServer.StartSimAsync();
		Assert.Empty(testServer.Log);
		await testServer.AppendEntriesAsync(new AppendEntry(1, "HB", 1, Operation.Default, 0, new List<LogEntry>() { new LogEntry(1, Operation.Default, "test request from client") }, 1));
		await fake1.Received().ConfirmAsync(new ConfirmationDTO(1,3,0));
	}
	
}
