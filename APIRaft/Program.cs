using System.Text.Json;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using RaftClassLib;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:8080");


var nodeId = Environment.GetEnvironmentVariable("NODE_ID") ?? throw new Exception("NODE_ID environment variable not set");
var otherNodesRaw = Environment.GetEnvironmentVariable("OTHER_NODES") ?? throw new Exception("OTHER_NODES environment variable not set");
var nodeIntervalScalarRaw = Environment.GetEnvironmentVariable("NODE_INTERVAL_SCALAR") ?? throw new Exception("NODE_INTERVAL_SCALAR environment variable not set");

builder.Services.AddLogging();
var serviceName = "Node" + nodeId;
builder.Logging.AddOpenTelemetry(options =>
{
    options
      .SetResourceBuilder(
          ResourceBuilder
            .CreateDefault()
            .AddService(serviceName)
      )
      .AddOtlpExporter(options =>
      {
          options.Endpoint = new Uri("http://dashboard:18889");
      });
});
var app = builder.Build();

var logger = app.Services.GetService<ILogger<Program>>();
logger.LogInformation("Node ID {name}", nodeId);
logger.LogInformation("Other nodes environment config: {}", otherNodesRaw);

INode[] otherNodes = otherNodesRaw
  .Split(";")
  .Select(s => new HttpRpcOtherNode(int.Parse(s.Split(",")[0]), s.Split(",")[1]))
  .ToArray();


logger.LogInformation("other nodes {nodes}", JsonSerializer.Serialize(otherNodes));


//var node = new RaftNode(otherNodes)
//{
//    Id = int.Parse(nodeId),
//    logger = app.Services.GetService<ILogger<RaftNode>>()
//};
var node = new ServerAaron(int.Parse(nodeId))
{
    //OtherServers = otherNodes,
    NetworkDelayModifier = 50,
};

//RaftNode.NodeIntervalScalar = double.Parse(nodeIntervalScalarRaw);
await node.StartSimAsync();

app.MapGet("/health", () => "healthy");

app.MapGet("/nodeData", () =>
{
    return new NodeData(
      Id: node.ID,
      Status: node.IsLive,
      ElectionTimeout: node.ElectionTimer.Interval,
      Term: node.Term,
      CurrentTermLeader: node.LeaderId,
      CommittedEntryIndex: node.commitIndex,
      Log: node.Log,
      State: node.State,
      NodeIntervalScalar: node.NetworkDelayModifier
    );
});

app.MapPost("AppendEntries", async (AppendEntry Entry) => {
    logger.LogInformation("received append entries {Entry}", Entry);
    await node.AppendEntriesAsync(Entry);
});
app.MapPost("StartSim", async (string junk) =>
{
    logger.LogInformation("received StartSim for node {nodeId}", node.ID);
	await node.StartSimAsync();
});
app.MapPost("StopAsync", async (string junk) =>
{
    await node.StopAsync();
});
//Task RequestVoteAsync(RequestVoteDTO request);
app.MapPost("RequestVote", async (RequestVoteDTO vote) => {
	logger.LogInformation("received vote Request {vote}", vote);
	await node.RequestVoteAsync(vote);
});
//Task ConfirmAsync(int term, int reciverId, int indexOfLog = 0);
//Task HBRecivedAsync(int reciverId);
//Task ReciveVoteAsync(int senderID, bool v);
//Task StartSimAsync();
//Task ClientRequestAsync(string value);
app.Run();