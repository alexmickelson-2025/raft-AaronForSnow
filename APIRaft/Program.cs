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

IServerAaron[] otherNodes = otherNodesRaw
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
    OtherServers = otherNodes.ToList(),
    NetworkDelayModifier = 50,
    ElectionTimeoutMultiplier = 50,
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

app.MapPost("/AppendEntries", async (AppendEntry Entry) => {
    logger.LogInformation("received append entries {Entry}", Entry);
    await node.AppendEntriesAsync(Entry);
});
app.MapPost("/StartSim", async (string junk) =>
{
    logger.LogInformation("received StartSim for node {nodeId}", node.ID);
	await node.StartSimAsync();
});
app.MapPost("/StopAsync", async (string junk) =>
{
    await node.StopAsync();
});
app.MapPost("/RequestVote", async (RequestVoteDTO vote) => {
	logger.LogInformation("received vote Request {vote}", vote);
	await node.RequestVoteAsync(vote);
});
app.MapPost("/Confirm", async (ConfirmationDTO confirm) =>
{
    logger.LogInformation("received confirmation {confirmaiton}", confirm);
    await node.ConfirmAsync(confirm);
});
app.MapPost("/HBReceived", async (int receiverId) => {
    logger.LogInformation("received Heart Beat from {receiverId}", receiverId );
    await node.HBReceivedAsync(receiverId);
});
app.MapPost("/ReceiveVote", async (ReceiveVoteDTO ballet) => {
    logger.LogInformation("received vote ballet {ballet}", ballet);
    await node.ReceiveVoteAsync(ballet);
});
app.MapPost("/ClientRequest", async (string request) => {
    logger.LogInformation("received Client Request {request}", request);
    await node.ClientRequestAsync(request);
});
app.Run();