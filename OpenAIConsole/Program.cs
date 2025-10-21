
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAIConsole.Configuration;
using OpenAIConsole.Services;
using OpenAIConsole.Interfaces;


var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? "Production";

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .Build();


var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole());

// Configure OpenAI settings
services.Configure<OpenAIConfig>(configuration.GetSection("OpenAI"));

// Setup paths
var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data");
Directory.CreateDirectory(dataDirectory);
var mockEmailsPath = Path.Combine(dataDirectory, "mock-emails.json");



// Get number of mock emails from user with validation
int emailCount = 100;
while (true)
{
    Console.Write("Enter the number of mock emails to generate (default: 100): ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        emailCount = 100;
        break;
    }
    if (int.TryParse(input, out emailCount) && emailCount > 0)
    {
        break;
    }
    Console.WriteLine("Invalid input. Please enter a positive integer.");
}

// Prompt for batch size, suggest not over 10
int batchSize = 5;
while (true)
{
    Console.Write("Enter batch size for parallel summarization (suggested: 5, max: 10): ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        batchSize = 5;
        break;
    }
    if (int.TryParse(input, out batchSize) && batchSize > 0 && batchSize <= 10)
    {
        break;
    }
    Console.WriteLine("Invalid input. Please enter a positive integer not greater than 10.");
}


// Register services with Interfaces namespace
services.AddSingleton<IEmailDataProvider>(sp => new MockEmailDataProvider(mockEmailsPath, true, emailCount));
services.AddSingleton<IOpenAIService, OpenAIService>();
services.AddSingleton<IEmailFileService, EmailFileService>();
services.AddSingleton<IEmailSummarizerService>(sp => new EmailSummarizerService(
    sp.GetRequiredService<IOpenAIService>(),
    sp.GetRequiredService<IEmailDataProvider>(),
    sp.GetRequiredService<IEmailFileService>()
));


var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("EmailSummarizer");
var emailSummarizer = serviceProvider.GetRequiredService<IEmailSummarizerService>();

logger.LogInformation("Email Summarizer Demo");
logger.LogInformation("1. Loading mock emails...");

try
{
    var emails = await emailSummarizer.GetEmailsAsync();
    logger.LogInformation($"Loaded {emails.Count} emails.");

    logger.LogInformation("2. Starting email summarization...");
    Console.WriteLine("Press any key to begin summarizing emails, or 'Esc' to exit.");

    if (Console.ReadKey().Key != ConsoleKey.Escape)
    {
        await emailSummarizer.SummarizeAllEmailsAsync(-1, batchSize);
        logger.LogInformation("Email summarization completed!");
        logger.LogInformation("Original emails remain in: Data/mock-emails.json");
        logger.LogInformation("Important and non-important emails have been saved to separate files");
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred during email summarization.");
    Console.WriteLine($"\nError: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();