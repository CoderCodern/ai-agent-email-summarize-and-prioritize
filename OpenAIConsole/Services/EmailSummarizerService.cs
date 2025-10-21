
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
        Task SummarizeAllEmailsAsync(int maxEmails = -1, int batchSize = 5);
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

        // AnalyzeEmail with chunking and hierarchical summarization for long emails
        private async Task<EmailAnalysisResult> AnalyzeEmail(Email email, int maxTokens = 1500)
        {
            // Estimate token count (rough: 1 token ≈ 4 chars)
            int tokenLimit = maxTokens;
            string content = email.Content;
            int chunkSize = tokenLimit * 4; // rough char count per chunk

            // Split content into chunks
            var chunks = new List<string>();
            for (int i = 0; i < content.Length; i += chunkSize)
                chunks.Add(content.Substring(i, Math.Min(chunkSize, content.Length - i)));

            // If only one chunk, analyze and return directly
            if (chunks.Count == 1)
                return await AnalyzeEmailSingle(email);

            Console.WriteLine($"Chunking email '{email.Subject}' (length: {content.Length}) into {chunks.Count} chunks of {chunkSize} chars.");

            // Analyze each chunk and collect results
            var chunkResults = new List<EmailAnalysisResult>(chunks.Count);
            var chunkSummaries = new List<string>(chunks.Count);
            foreach (var chunk in chunks)
            {
                if (chunk.Length > chunkSize)
                    Console.WriteLine($"Warning: Chunk size exceeded for chunk in '{email.Subject}'. Length: {chunk.Length}");
                var chunkEmail = new Email
                {
                    From = email.From,
                    To = email.To,
                    Subject = email.Subject,
                    Type = email.Type,
                    Content = chunk
                };
                var result = await AnalyzeEmailSingle(chunkEmail);
                if (result == null || string.IsNullOrWhiteSpace(result?.Summary))
                    Console.WriteLine($"Warning: Empty or invalid AI response for chunk in '{email.Subject}'.");
                chunkResults.Add(result ?? new EmailAnalysisResult { Summary = "", IsImportant = false, ImportanceReason = "No AI response", PromptTokens = 0, CompletionTokens = 0, TotalTokens = 0, Cost = 0.0 });
                chunkSummaries.Add(result?.Summary ?? "");
            }

            // Hierarchical summarization: summarize the chunk summaries
            var summaryContent = string.Join("\n", chunkSummaries);
            var summaryEmail = new Email
            {
                From = email.From,
                To = email.To,
                Subject = email.Subject,
                Type = email.Type,
                Content = summaryContent
            };
            var finalResult = await AnalyzeEmailSingle(summaryEmail);

            // Aggregate token usage and cost
            int? promptTokens = chunkResults.Sum(r => r.PromptTokens ?? 0) + (finalResult.PromptTokens ?? 0);
            int? completionTokens = chunkResults.Sum(r => r.CompletionTokens ?? 0) + (finalResult.CompletionTokens ?? 0);
            int? totalTokens = chunkResults.Sum(r => r.TotalTokens ?? 0) + (finalResult.TotalTokens ?? 0);
            double? cost = chunkResults.Sum(r => r.Cost ?? 0) + (finalResult.Cost ?? 0);

            return new EmailAnalysisResult
            {
                Summary = finalResult.Summary,
                IsImportant = finalResult.IsImportant,
                ImportanceReason = finalResult.ImportanceReason,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = totalTokens,
                Cost = cost
            };
        }

        // The original AnalyzeEmail logic, now private for chunking use
        private async Task<EmailAnalysisResult> AnalyzeEmailSingle(Email email)
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
                        ImportanceReason = fallbackReason,
                        PromptTokens = 0,
                        CompletionTokens = 0,
                        TotalTokens = 0,
                        Cost = 0.0
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

        public async Task SummarizeAllEmailsAsync(int maxEmails = -1, int batchSize = 5)
        {
            var emails = await GetEmailsAsync();
            if (maxEmails > 0)
                emails = emails.Take(maxEmails).ToList();

            var summarizedEmails = new List<Email>();
            for (int i = 0; i < emails.Count; i += batchSize)
            {
                var batch = emails.Skip(i).Take(batchSize).ToList();
                var tasks = batch.Select(async email => {
                    var analysis = await AnalyzeEmail(email);
                    email.Summary = analysis.Summary;
                    email.IsImportant = analysis.IsImportant;
                    email.ImportanceReason = analysis.ImportanceReason;
                    email.PromptTokens = analysis.PromptTokens;
                    email.CompletionTokens = analysis.CompletionTokens;
                    email.TotalTokens = analysis.TotalTokens;
                    email.Cost = analysis.Cost;
                    return email;
                });
                var results = await Task.WhenAll(tasks);
                summarizedEmails.AddRange(results);
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