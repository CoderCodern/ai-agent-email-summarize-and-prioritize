namespace OpenAIConsole.Configuration;

public class OpenAIConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int MaxTokens { get; set; }
    public float Temperature { get; set; }
}