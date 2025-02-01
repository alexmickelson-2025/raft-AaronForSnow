using RaftClassLib;

public class HttpRpcOtherNode : INode
{
    public int ID { get; }
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

    public Task RequestVoteAsync(RequestVoteDTO request)
    {
        throw new NotImplementedException();
    }

    public Task ReciveVoteAsync(ReceiveVoteDTO ballet)
    {
        throw new NotImplementedException();
    }

    public Task ConfirmAsync(ConfirmationDTO info)
    {
        throw new NotImplementedException();
    }

    public Task HBRecivedAsync(int reciverId)
    {
        throw new NotImplementedException();
    }
}