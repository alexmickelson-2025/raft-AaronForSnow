using RaftClassLib;

public class HttpRpcOtherNode : IServerAaron
{
    public int ID { get; set; }
    public string Url { get; }
    private HttpClient client = new();

    public HttpRpcOtherNode(int id, string url)
    {
        ID = id;
        Url = url;
    }

    public async Task AppendEntriesAsync(AppendEntry Entry)
    {
        await client.PostAsJsonAsync(Url + "AppendEntries", Entry);
    }

    public async Task StartSimAsync()
    {
        await client.PostAsJsonAsync(Url + "StartSim", "");
    }

    public async Task StopAsync()
    {
        await client.PostAsJsonAsync(Url + "StopAsync", "");
    }

    public async Task RequestVoteAsync(RequestVoteDTO request)
    {
        await client.PostAsJsonAsync(Url + "RequestVote", request);
    }

    public async Task ReceiveVoteAsync(ReceiveVoteDTO ballet)
    {
        await client.PostAsJsonAsync(Url + "ReceiveVote", ballet);
    }

    public async Task ConfirmAsync(ConfirmationDTO info)
    {
        await client.PostAsJsonAsync(Url + "Confirm", info);
    }

    public async Task HBReceivedAsync(int reciverId)
    {
        await client.PostAsJsonAsync(Url + "HBReceived", reciverId);
    }

	public async Task ClientRequestAsync(string value)
	{
        await client.PostAsJsonAsync(Url + "ClientRequest", value);
	}
}