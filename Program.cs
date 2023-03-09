// See https://aka.ms/new-console-template for more information

var configs = Configurations.LoadConfigurations();
InitializeGraph(configs);

try {
  var user = await MSGraphMailService.getUser();
  Console.WriteLine($"Welcome {user?.DisplayName}, {user?.Mail ?? user?.UserPrincipalName ?? "No Other info retrieved"}");
} catch (Exception) {
  throw new Exception("Error finding user");
}


int waiting = -1;
string? userInput = "";
while (waiting != 0) {
  if (waiting == 1) {
    Console.WriteLine("Do you want to delete above emails? Please enter 'Yes' or 'No'");
  }
  else {
    Console.WriteLine("Please enter a domain name which you want to filter on, or enter q to quit.");
  }
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
  } 
  else if (waiting == 1 && userInput == "Yes") {
    try {
      DeleteKEmailsByDomain();
      waiting = -1;
    }
    catch (Exception exception) {
      Console.WriteLine($"Error: {exception}");
    }
  } 
  else if (waiting == 1  && userInput == "No") {
    waiting = 1;
  }
  else {
    try {
      ListKEmailsByDomain(2, userInput);
      waiting = 1;
    }
    catch (Exception exception) {
      waiting = -1;
      Console.WriteLine($"Error: {exception}");
    }
  }

}

Console.WriteLine("Thank you for using the service!");

void InitializeGraph(Configurations configurations) {
  MSGraphMailService.InitializeForUserAuth(configurations, (info, cancel) => {
    Console.WriteLine(info.Message);
    return Task.FromResult(0);
  });
}

/*
string ConstructDomainFilter(string userInput) {
  return "";
}
*/
void ListKEmailsByDomain(int topK , string domainFilter) {
  try {
    MSGraphMailService.getTopKMessageByDomain(topK, domainFilter);
  } 
  catch (Exception ex) {
    Console.WriteLine(ex.Message);
  }
}

void DeleteKEmailsByDomain() {
  try {
    MSGraphMailService.deleteTopKMessageByDomain();
  } 
  catch (Exception ex) {
    Console.WriteLine(ex.Message);

  }
}