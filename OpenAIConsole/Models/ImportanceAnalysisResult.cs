namespace OpenAIConsole.Models;

public class ImportanceAnalysisResult
{
    public bool IsImportant { get; set; }
    public string Reason { get; set; } = string.Empty;
}