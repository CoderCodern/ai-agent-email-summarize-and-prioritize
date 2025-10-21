
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAIConsole.Configuration;
using OpenAIConsole.Models;
using OpenAIConsole.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenAIConsole.Services
{
    public interface IEmailSummarizerService
    {
        Task<List<Email>> GetEmailsAsync();
        Task<string> SummarizeEmailAsync(Email email);
        Task SummarizeAllEmailsAsync(int maxEmails = -1);
        Task SaveSummarizedEmailsAsync(List<Email> summarizedEmails);
    }

    public class EmailSummarizerService : IEmailSummarizerService
    {
        private readonly IOpenAIService _openAIService;
        private readonly IEmailDataProvider _emailDataProvider;
        private readonly IEmailFileService _emailFileService;
        private readonly string _importantEmailsPath;
        private readonly string _nonImportantEmailsPath;

        public EmailSummarizerService(
            IOpenAIService openAIService,
            IEmailDataProvider emailDataProvider,
            IEmailFileService emailFileService)
        {
            _openAIService = openAIService;
            _emailDataProvider = emailDataProvider;
            _emailFileService = emailFileService;
            var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            Directory.CreateDirectory(dataDirectory);
            _importantEmailsPath = Path.Combine(dataDirectory, "important-emails.json");
            _nonImportantEmailsPath = Path.Combine(dataDirectory, "non-important-emails.json");
        }

        private class EmailAnalysisResult
        {
            [JsonPropertyName("summary")]
            public string Summary { get; set; } = string.Empty;
            [JsonPropertyName("isImportant")]
            public bool IsImportant { get; set; }
            [JsonPropertyName("importanceReason")]
            public string ImportanceReason { get; set; } = string.Empty;
            [JsonPropertyName("promptTokens")]
            public int? PromptTokens { get; set; }
            [JsonPropertyName("completionTokens")]
            public int? CompletionTokens { get; set; }
            [JsonPropertyName("totalTokens")]
            public int? TotalTokens { get; set; }
            [JsonPropertyName("cost")]
            public double? Cost { get; set; }
        }

        private async Task<EmailAnalysisResult> AnalyzeEmail(Email email)
        {
            try
            {
                var systemPrompt = @"You are an expert business email analyst. Your job is to:
                    1. Read the email content, subject, sender, and type.
                    2. Write a concise, actionable summary (1-2 sentences, no generic phrases).
                    3. Judge importance based on urgency, business impact, sender authority, and required actions.
                    4. Give a detailed reason for your judgment, referencing specific details from the email.
                    Respond ONLY with a valid JSON object with these fields: summary, isImportant, importanceReason. Do not include any extra text, comments, or formatting. Do not use markdown. Do not explain your answer. Output ONLY the JSON object.";

                var prompt = $@"Email Details:
                    From: {email.From}
                    To: {email.To}
                    Subject: {email.Subject}
                    Type: {email.Type}
                    Content:
                    {email.Content}

                    Instructions:
                    - Summarize the key points and required actions in 1-2 sentences.
                    - Is this email important? Consider urgency, business impact, sender authority, and required actions.
                    - Explain your judgment with specific references to the email.

                    Respond ONLY with a valid JSON object. The fields must be: summary, isImportant, importanceReason. Do not include any extra text, comments, or formatting. Do not use markdown. Do not explain your answer. Output ONLY the JSON object.";

                var (response, usage) = await _openAIService.GetCompletionWithUsageAsync(systemPrompt + "\n" + prompt);
                Console.WriteLine($"AI raw response:\n{response}");
                EmailAnalysisResult? result = null;
                try
                {
                    result = JsonSerializer.Deserialize<EmailAnalysisResult>(response);
                }
                catch (Exception directEx)
                {
                    int start = response.IndexOf('{');
                    int end = response.LastIndexOf('}');
                    if (start >= 0 && end > start)
                    {
                        var json = response.Substring(start, end - start + 1);
                        try
                        {
                            result = JsonSerializer.Deserialize<EmailAnalysisResult>(json);
                        }
                        catch (Exception jsonEx)
                        {
                            Console.WriteLine($"JSON parse error: {jsonEx.Message}\nJSON: {json}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Direct JSON parse error: {directEx.Message}\nResponse: {response}");
                    }
                }

                // Save token usage and cost
                if (result != null && usage != null)
                {
                    result.PromptTokens = usage.PromptTokens;
                    result.CompletionTokens = usage.CompletionTokens;
                    result.TotalTokens = usage.TotalTokens;
                    double promptCost = (usage.PromptTokens ?? 0) * 0.0015 / 1000.0;
                    double completionCost = (usage.CompletionTokens ?? 0) * 0.002 / 1000.0;
                    result.Cost = Math.Round(promptCost + completionCost, 6);
                }

                // Validate result
                if (result == null || string.IsNullOrWhiteSpace(result.Summary) || string.IsNullOrWhiteSpace(result.ImportanceReason))
                {
                    // Fallback: Use subject and first line of content
                    var fallbackSummary = $"{email.Subject}: {email.Content.Split('\n')[0]}";
                    var fallbackReason = $"Could not parse AI response. Judged by subject and sender: {email.Subject}, {email.From}";
                    var nonImportantTypes = new[] { "spam", "promotional", "newsletter", "notification" };
                    var isImportant = !nonImportantTypes.Contains(email.Type.ToLower());
                    return new EmailAnalysisResult
                    {
                        Summary = fallbackSummary,
                        IsImportant = isImportant,
                        ImportanceReason = fallbackReason
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AnalyzeEmail error: {ex.Message}");
                // Fallback: Use subject and first line of content
                var fallbackSummary = $"{email.Subject}: {email.Content.Split('\n')[0]}";
                var fallbackReason = $"Could not analyze email due to error. Judged by subject and sender: {email.Subject}, {email.From}";
                var nonImportantTypes = new[] { "spam", "promotional", "newsletter", "notification" };
                var isImportant = !nonImportantTypes.Contains(email.Type.ToLower());
                return new EmailAnalysisResult
                {
                    Summary = fallbackSummary,
                    IsImportant = isImportant,
                    ImportanceReason = fallbackReason
                };
            }
        }

        public async Task<List<Email>> GetEmailsAsync()
        {
            return await _emailDataProvider.GetEmailsAsync();
        }

        public async Task<string> SummarizeEmailAsync(Email email)
        {
            var analysis = await AnalyzeEmail(email);
            return analysis.Summary;
        }

        public async Task SummarizeAllEmailsAsync(int maxEmails = -1)
        {
            var emails = await GetEmailsAsync();
            if (maxEmails > 0)
                emails = emails.Take(maxEmails).ToList();

            var summarizedEmails = new List<Email>();
            foreach (var email in emails)
            {
                var analysis = await AnalyzeEmail(email);
                email.Summary = analysis.Summary;
                email.IsImportant = analysis.IsImportant;
                email.ImportanceReason = analysis.ImportanceReason;
                email.PromptTokens = analysis.PromptTokens;
                email.CompletionTokens = analysis.CompletionTokens;
                email.TotalTokens = analysis.TotalTokens;
                email.Cost = analysis.Cost;
                summarizedEmails.Add(email);
            }
            await SaveSummarizedEmailsAsync(summarizedEmails);
        }

        public async Task SaveSummarizedEmailsAsync(List<Email> summarizedEmails)
        {
            var importantEmails = new List<Email>();
            var nonImportantEmails = new List<Email>();

            foreach (var email in summarizedEmails)
            {
                if (email.IsImportant)
                    importantEmails.Add(email);
                else
                    nonImportantEmails.Add(email);
            }

            // Save important emails
            var importantCollection = new EmailCollection { Emails = importantEmails };
            var importantJson = JsonSerializer.Serialize(importantCollection, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_importantEmailsPath, importantJson);

            // Save non-important emails
            var nonImportantCollection = new EmailCollection { Emails = nonImportantEmails };
            var nonImportantJson = JsonSerializer.Serialize(nonImportantCollection, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_nonImportantEmailsPath, nonImportantJson);

            // Print summary
            Console.WriteLine($"\nEmail Classification Summary:");
            foreach (var email in summarizedEmails)
            {
                Console.WriteLine($"\nEmail: {email.Subject}");
                Console.WriteLine($"Classification: {(email.IsImportant ? "Important" : "Non-important")}");
                Console.WriteLine($"Reason: {email.ImportanceReason}");
            }
            Console.WriteLine($"\nTotal Important Emails: {importantEmails.Count}");
            Console.WriteLine($"Total Non-important Emails: {nonImportantEmails.Count}");
            Console.WriteLine($"Important emails saved to: {_importantEmailsPath}");
            Console.WriteLine($"Non-important emails saved to: {_nonImportantEmailsPath}");
        }
    }
}