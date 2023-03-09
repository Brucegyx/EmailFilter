using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;

class MSGraphMailService {
  private static Configurations? _configurations;
  private static DeviceCodeCredential? _deviceCodeCredential;

  private static GraphServiceClient? _graphServiceClient;

  private static List<string>? _messageIdsToDelete;

  public static void InitializeForUserAuth(Configurations configurations,
    Func<DeviceCodeInfo, CancellationToken, Task> deviceCodePrompt) 
  {
      _configurations = configurations;
      _deviceCodeCredential = new DeviceCodeCredential(deviceCodePrompt, configurations.TenantId, configurations.ClientId);
      _graphServiceClient = new GraphServiceClient(_deviceCodeCredential, configurations.GraphUserScopes);
      _messageIdsToDelete = new List<string>();
  }
  public static Task<User?> getUser() {
    _ = _graphServiceClient ?? throw new System.NullReferenceException("No graph service client");
    return _graphServiceClient.Me
      .GetAsync(requestConfiguration => requestConfiguration.QueryParameters.Select= new string[] {"DisplayName","Mail", "UserPrincipalName"});
  }

  public static async Task<string> getUserToken() {
    _ = _deviceCodeCredential ?? throw new System.NullReferenceException("No device code credential");
    _ = _configurations?.GraphUserScopes ?? throw new System.NullReferenceException("No user scope in settings");
    var context = new TokenRequestContext(_configurations.GraphUserScopes);
    var res = await _deviceCodeCredential.GetTokenAsync(context);
    return res.Token;

  }

  public static async Task getTopKMessageByDomain(int maximumTopK, string domain) {
    _ = _graphServiceClient ?? throw new System.NullReferenceException("No graph service client");
    var messages =  await _graphServiceClient.Me.Messages
      .GetAsync(
          requestConfig => {
          requestConfig.QueryParameters.Top = 1;
          requestConfig.QueryParameters.Select = new string[] { "sender", "subject", "receivedDateTime", "bodyPreview"};
          requestConfig.QueryParameters.Count = true;
          requestConfig.QueryParameters.Filter = $"contains(from/emailAddress/address, '{domain}')";
          requestConfig.Headers.Add("ConsistencyLevel","eventual");
        }
      );
    if (messages is null) {
      throw new System.NullReferenceException("No messages");
    }
    int count = 0;
    _messageIdsToDelete = new List<string>();
    var pagesIter = PageIterator<Message, MessageCollectionResponse>
      .CreatePageIterator(_graphServiceClient, messages,
       (msg) => {
        _messageIdsToDelete.Add(msg.Id ?? "");
        Console.WriteLine($"{count + 1}. ");
        Console.WriteLine($"  Sender from: {msg.Sender?.EmailAddress?.Address}");
        Console.WriteLine($"  Subject: {msg.Subject}");
        Console.WriteLine($"  Time received: {msg.ReceivedDateTime}");
        Console.WriteLine($"  Content: {msg.BodyPreview}");
        Console.WriteLine("");
        count++ ;
        if (count == maximumTopK) {return false;}
        return true;
       }, 
       (req) => {return req;});
    await pagesIter.IterateAsync();
  }

  public static async Task deleteTopKMessageByDomain() {
    _ = _graphServiceClient ?? throw new System.NullReferenceException("No graph service client");
    _messageIdsToDelete ??= new List<string> ();
    foreach (var messageId in _messageIdsToDelete) {
      if (messageId.Length > 0) {
        try {
          await _graphServiceClient.Me.Messages[messageId].DeleteAsync();
        } 
        catch (ServiceException ex) {
          Console.Write($"Error code: {ex.ResponseStatusCode}");
        }
      }
    }
  }
}