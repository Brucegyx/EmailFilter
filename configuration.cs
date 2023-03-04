using Microsoft.Extensions.Configuration;

public class Configurations {
  public string? ClientId { get; set; }
  public string? TenantId { get; set; }
  public string[]? GraphUserScope { get; set; }

  public static Configurations LoadConfigurations () {
    IConfiguration config = new ConfigurationBuilder()
      .AddJsonFile("settings.json", optional: false)
      .AddJsonFile($"settings.Development.json", optional: true)
      .Build();
    return config.GetRequiredSection("Settings").Get<Configurations>() ??
      throw new Exception("Settings not correctly loaded");
  }
}