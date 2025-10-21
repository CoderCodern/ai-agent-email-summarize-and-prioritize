using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAIConsole.Configuration;

namespace OpenAIConsole.Services;

public class UsageData
{
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public int? TotalTokens { get; set; }
}

public interface IOpenAIService
{
    Task<(string Completion, UsageData Usage)> GetCompletionWithUsageAsync(string prompt);
}

public class OpenAIService : IOpenAIService
{
    private readonly OpenAIClient _client;
    private readonly OpenAIConfig _config;

    public OpenAIService(IOptions<OpenAIConfig> config)
    {
        _config = config.Value;
        _client = new OpenAIClient(_config.ApiKey);
    }

    public async Task<(string Completion, UsageData Usage)> GetCompletionWithUsageAsync(string prompt)
    {
        var chatCompletions = await _client.GetChatCompletionsAsync(
            _config.Model,
            new ChatCompletionsOptions
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, "You are a helpful assistant."),
                    new ChatMessage(ChatRole.User, prompt)
                },
                MaxTokens = _config.MaxTokens,
                Temperature = _config.Temperature
            });

        var choice = chatCompletions.Value.Choices[0].Message.Content;
        var usage = chatCompletions.Value.Usage;
        var usageData = new UsageData
        {
            PromptTokens = usage?.PromptTokens,
            CompletionTokens = usage?.CompletionTokens,
            TotalTokens = usage?.TotalTokens
        };
        return (choice, usageData);
    }
}