using System.Text.Json.Serialization;

namespace OpenAIConsole.Models;

public class Email
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;
    
    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;
    
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;
    
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
    
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("isImportant")]
    public bool IsImportant { get; set; }

    [JsonPropertyName("importanceReason")]
    public string? ImportanceReason { get; set; }

    [JsonPropertyName("promptTokens")]
    public int? PromptTokens { get; set; }

    [JsonPropertyName("completionTokens")]
    public int? CompletionTokens { get; set; }

    [JsonPropertyName("totalTokens")]
    public int? TotalTokens { get; set; }

    [JsonPropertyName("cost")]
    public double? Cost { get; set; }
}

public class EmailCollection
{
    [JsonPropertyName("emails")]
    public List<Email> Emails { get; set; } = new();
}