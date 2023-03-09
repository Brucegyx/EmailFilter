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
    Console.WriteLine("Please enter a domain name on which you want to filter ");
    Console.WriteLine("And a number of emails to display");
    Console.WriteLine("In the format: <domain name> <number>");
    Console.WriteLine("For example: enter '@microsoft.com 5' to get five emails sent from any address with domain microsoft.com");
    Console.WriteLine("Or enter 'q' to quit.");
  }
  try {
    userInput = Console.ReadLine();
  } catch (IOException exception){
    Console.WriteLine(exception.Message);
    waiting = -1;
  }
  if (userInput is null) {
    break;
  }
  if (userInput == "q") {
    break;
  } 
  if (waiting == 1 && userInput == "Yes") {
    try {
      await DeleteKEmailsByDomain();
      waiting = -1;
      Console.WriteLine("Your selected emails were successfully deleted! ");
    }
    catch (Exception exception) {
      Console.WriteLine($"Error: {exception}");
    }
  } 
  else if (waiting == 1  && userInput == "No") {
    waiting = -1;
  }
  else {
    string[] inputs = userInput.Split(' ');
    string domainToFilter = inputs[0];
    try {    
      int topK = int.Parse(inputs[1]);
      await ListKEmailsByDomain(topK, domainToFilter);
      waiting = 1; // indicate user to choose to delete the retreived emails
    }
    catch (FormatException ex) {
      Console.WriteLine($"Format Error: input {inputs[1]} is not a valid number.");
      Console.WriteLine($"Error: {ex.Message}");
      waiting = -1;
    }
    catch (Exception exception) {
      Console.WriteLine($"Error: {exception}");
      waiting = -1;
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

async Task ListKEmailsByDomain(int topK , string domainFilter) {
  try {
    await MSGraphMailService.getTopKMessageByDomain(topK, domainFilter);
  } 
  catch (Exception ex) {
    Console.WriteLine(ex.Message);
  }
}

async Task DeleteKEmailsByDomain() {
  try {
    await MSGraphMailService.deleteTopKMessageByDomain();
  } 
  catch (Exception ex) {
    Console.WriteLine(ex.Message);

  }
}