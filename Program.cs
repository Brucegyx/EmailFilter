// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var configs = Configurations.LoadConfigurations();
InitializeGraph(configs);

try {
  var user = await MSGraph.getUser();
  Console.WriteLine($"Welcome {user?.DisplayName}");
} catch (Exception) {
  throw new Exception("Error finding user");
}

Console.WriteLine("Please enter the domain name you want to filter or enter q to quit.");

int waiting = -1;
string? userInput = "";
while (waiting != 0) {
  try {
    userInput = Console.ReadLine();
  } catch (IOException exception){
    Console.WriteLine(exception.Message);
    waiting = -1;
  }
  if (userInput is null) {
    waiting = 0;
    continue;
  }
  if (userInput == "q") {
    waiting = 0;
    break;
  } else {
    try {
      await ListEmailsByDomain(5, userInput);
    }
    catch (Exception exception) {
      Console.WriteLine($"Error: {exception.Message}");
    }
  }

}

Console.WriteLine("Thank you for using the service!");

void InitializeGraph(Configurations configurations) {
  MSGraph.InitializeForUserAuth(configurations, (info, cancel) => {
    Console.WriteLine(info.Message);
    return Task.FromResult(0);
  });
}

string ConstructDomainFilter(string userInput) {
  return "";
}

async Task ListEmailsByDomain(int top , string domainFilter) {
  var messages = await MSGraph.getTopKMessageByDomain(top, domainFilter);
  Console.WriteLine(messages?.ToString());
}