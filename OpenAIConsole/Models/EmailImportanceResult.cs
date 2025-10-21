using System.Text.Json.Serialization;

namespace OpenAIConsole.Models;

public class EmailImportanceResult
{
    [JsonPropertyName("isImportant")]
    public bool IsImportant { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
}