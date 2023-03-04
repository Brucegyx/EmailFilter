using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;

class MSGraph {
  private static Configurations? _configurations;
  private static DeviceCodeCredential? _deviceCodeCredential;

  private static GraphServiceClient? _graphServiceClient;

  public static void InitializeForUserAuth(Configurations configurations,
    Func<DeviceCodeInfo, CancellationToken, Task> deviceCodePrompt) 
  {
      _configurations = configurations;
      _deviceCodeCredential = new DeviceCodeCredential(deviceCodePrompt, configurations.TenantId, configurations.ClientId);
      _graphServiceClient = new GraphServiceClient(_deviceCodeCredential, configurations.GraphUserScope);
  }
  public static Task<User?> getUser() {
    _ = _graphServiceClient ?? throw new System.NullReferenceException("No graph service client");
    return _graphServiceClient.Me
      .GetAsync(requestConfiguration => requestConfiguration.QueryParameters.Select= new string[] {"DisplayName","Email"});
  }

  public static Task<MessageCollectionResponse?> getTopKMessageByDomain(int maximumTopK, string domain) {
    _ = _graphServiceClient ?? throw new System.NullReferenceException("No graph service client");
    return _graphServiceClient.Me.Messages
      .GetAsync(requestConfig => {
        requestConfig.QueryParameters.Top = maximumTopK;
        requestConfig.QueryParameters.Select = new string[] {"sender", "body"};
        requestConfig.QueryParameters.Count = true;
        requestConfig.QueryParameters.Filter = $"from/emailAddress/address contains '{domain}'";
      });
    
  }
}