using RaftClassLib;

public class HttpRpcOtherNode : INode
{
    public int Id { get; }
    public string Url { get; }
    private HttpClient client = new();

    public HttpRpcOtherNode(int id, string url)
    {
        Id = id;
        Url = url;
    }

    //public async Task RequestAppendEntries(AppendEntriesData request)
    //{
    //    try
    //    {
    //        await client.PostAsJsonAsync(Url + "/request/appendEntries", request);
    //    }
    //    catch (HttpRequestException)
    //    {
    //        Console.WriteLine($"node {Url} is down");
    //    }
    //}

    //public async Task RequestVote(VoteRequestData request)
    //{
    //    try
    //    {
    //        await client.PostAsJsonAsync(Url + "/request/vote", request);
    //    }
    //    catch (HttpRequestException)
    //    {
    //        Console.WriteLine($"node {Url} is down");
    //    }
    //}

    //public async Task RespondAppendEntries(RespondEntriesData response)
    //{
    //    try
    //    {
    //        await client.PostAsJsonAsync(Url + "/response/appendEntries", response);
    //    }
    //    catch (HttpRequestException)
    //    {
    //        Console.WriteLine($"node {Url} is down");
    //    }
    //}

    //public async Task ResponseVote(VoteResponseData response)
    //{
    //    try
    //    {
    //        await client.PostAsJsonAsync(Url + "/response/vote", response);
    //    }
    //    catch (HttpRequestException)
    //    {
    //        Console.WriteLine($"node {Url} is down");
    //    }
    //}

    //public async Task SendCommand(ClientCommandData data)
    //{
    //    await client.PostAsJsonAsync(Url + "/request/command", data);
    //}

    public async Task AppendEntriesAsync(AppendEntry Entry)
    {
        await client.PostAsJsonAsync(Url + "AppendEntries", Entry);
    }

    public async Task StartSimAsync()
    {
        await client.PostAsJsonAsync(Url + "StartSim", "");
    }

    public Task StopAsync()
    {
        throw new NotImplementedException();
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