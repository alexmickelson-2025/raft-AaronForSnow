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
        try {

        await client.PostAsJsonAsync(Url + "/AppendEntries", Entry);
        }
        catch {
            Console.WriteLine($"AppentEntry failed for node{ID}");
        }
    }

    public async Task StartSimAsync()
    {
        try {
        await client.PostAsJsonAsync(Url + "/StartSim", "");
        }
        catch {
            Console.WriteLine($"StartSimAsync failed for node{ID}");
        }
    }

    public async Task StopAsync()
    {
        try {
        await client.PostAsJsonAsync(Url + "/StopAsync", "");
        }
        catch {
            Console.WriteLine($"StopAsync failed for node{ID}");
        }
    }

    public async Task RequestVoteAsync(RequestVoteDTO request)
    {
        try {
            await client.PostAsJsonAsync(Url + "/RequestVote", request);
        }
        catch {
            Console.WriteLine($"RequestVote failed for node{ID}");

        }
    }

    public async Task ReceiveVoteAsync(ReceiveVoteDTO ballet)
    {
        try {
            await client.PostAsJsonAsync(Url + "/ReceiveVote", ballet);
        }
        catch {
            Console.WriteLine($"ReceiveVote failed for node{ID}");
        }
    }

    public async Task ConfirmAsync(ConfirmationDTO info)
    {
        try {
        await client.PostAsJsonAsync(Url + "/Confirm", info);
        }
        catch {
            Console.WriteLine($"Confirm failed for node{ID}");
        }
    }

    public async Task HBReceivedAsync(int ReceiverId)
    {
        try {
            await client.PostAsJsonAsync(Url + $"/HBReceived/" + ReceiverId, "");
        }
        catch {
            Console.WriteLine($"HBReceived failed for node{ID}");
        }
    }

	public async Task ClientRequestAsync(string value)
	{
        try {
            await client.PostAsJsonAsync(Url + "/ClientRequest/" + value , "");
        }
        catch {
            Console.WriteLine($"ClientRequest failed for node{ID}");
        }
	}
}