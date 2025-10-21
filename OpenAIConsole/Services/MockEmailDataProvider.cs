using OpenAIConsole.Models;
using System.Text.Json;
using OpenAIConsole.Interfaces;

namespace OpenAIConsole.Services;

public class MockEmailDataProvider : IEmailDataProvider
{
    private readonly string _dataPath;
    private readonly bool _regenerateOnEveryRun;
    private readonly int _emailCount;

    public MockEmailDataProvider(string dataPath, bool regenerateOnEveryRun = false, int emailCount = 100)
    {
        _dataPath = dataPath;
        _regenerateOnEveryRun = regenerateOnEveryRun;
        _emailCount = emailCount;
    }

    public async Task<List<Email>> GetEmailsAsync()
    {
        if (_regenerateOnEveryRun || !File.Exists(_dataPath))
        {
            var emails = GenerateMockEmails(_emailCount);
            await SaveEmailsAsync(emails);
            return emails;
        }

        var jsonContent = await File.ReadAllTextAsync(_dataPath);
        
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            throw new InvalidOperationException($"Email data file is empty: {_dataPath}");
        }

        try
        {
            var emailCollection = JsonSerializer.Deserialize<EmailCollection>(jsonContent);
            return emailCollection?.Emails ?? new List<Email>();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse email data file: {ex.Message}");
        }
    }

    public async Task SaveEmailsAsync(List<Email> emails)
    {
        var directory = Path.GetDirectoryName(_dataPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(_dataPath, 
            JsonSerializer.Serialize(new EmailCollection { Emails = emails }, 
            new JsonSerializerOptions { WriteIndented = true }));
    }

    private List<Email> GenerateMockEmails(int count = 100)
    {
        var mockEmails = new List<Email>();
        var random = new Random(42); // Fixed seed for reproducibility
        var types = new[] { "business", "alert", "newsletter", "promotional", "personal", "notification", "billing", "calendar", "spam" };
        var domains = new[] { "company.com", "tech.com", "service.com", "client.com", "vendor.com", "platform.com" };
        var names = new[] { "john", "alice", "bob", "sarah", "mike", "emma", "david", "lisa", "james", "anna" };
        var subjects = new[]
        {
            "Project Update: {0}",
            "Meeting Reminder: {0}",
            "Alert: System {0}",
            "Weekly Newsletter: {0}",
            "Action Required: {0}",
            "Report: {0} Summary",
            "Notification: {0}",
            "Updates to {0}",
            "Review Request: {0}",
            "Important: {0} Status"
        };
        var topics = new[]
        {
            "Q4 Planning",
            "System Performance",
            "Team Collaboration",
            "Security Protocol",
            "Customer Feedback",
            "Infrastructure Update",
            "Development Sprint",
            "Budget Review",
            "Product Launch",
            "API Integration"
        };


        for (int i = 1; i <= count; i++)
        {
            var type = types[random.Next(types.Length)];
            var domain = domains[random.Next(domains.Length)];
            var fromName = names[random.Next(names.Length)];
            var subjectTemplate = subjects[random.Next(subjects.Length)];
            var topic = topics[random.Next(topics.Length)];

            string content = GenerateContent(type, topic);
            // For the first 4 emails, make the content very long for chunking test
            if (i <= 4)
            {
                content += "\n" + new string((char)('A' + (i - 1)), 2000);
                content += "\n" + new string((char)('E' + (i - 1)), 2000);
                content += "\n" + new string((char)('I' + (i - 1)), 2000);
            }

            var email = new Email
            {
                Id = $"e{i:D03}",
                From = $"{fromName}@{domain}",
                To = "john.doe@company.com",
                Subject = string.Format(subjectTemplate, topic),
                Date = DateTime.UtcNow.AddHours(-i),
                Type = type,
                Content = content
            };

            mockEmails.Add(email);
        }

        return mockEmails;
    }

    private string GenerateContent(string type, string topic)
    {
        return type switch
        {
            "business" => $"Hi team,\n\nRegarding the {topic}, here are the key updates:\n1. Timeline reviewed and approved\n2. Resources allocated\n3. Milestones defined\n\nPlease review and provide your feedback by EOD.\n\nBest regards,\nProject Team",
            "alert" => $"ALERT: {topic} requires immediate attention.\n\nIssue detected:\n- Severity: High\n- Impact: Service availability\n- Status: Under investigation\n\nAction required: Please review and respond within 2 hours.",
            "newsletter" => $"This Week's {topic} Update:\n\n1. Latest Developments\n2. Industry Trends\n3. Upcoming Events\n\nClick here to read more.\nTo unsubscribe, click here.",
            "promotional" => $"🎉 Special Offer!\n\nDon't miss out on our {topic} promotion:\n- 50% off for first-time users\n- Limited time offer\n- Premium features included\n\nClick to learn more!",
            "personal" => $"Hey,\n\nJust wanted to check in about {topic}. Let's catch up soon to discuss the details.\n\nCheers!",
            "notification" => $"System Notification: {topic}\n\nStatus: Updated\nChanges detected: 3\nAction required: Review changes\n\nClick to view details.",
            "billing" => $"Your {topic} Invoice\n\nAmount: $199.99\nDue Date: Next week\nService Period: Current month\n\nClick here to view and pay.",
            "calendar" => $"{topic} Meeting\n\nDate: Tomorrow\nTime: 10:00 AM EST\nLocation: Conference Room A\n\nAgenda:\n1. Review\n2. Planning\n3. Next Steps",
            _ => $"Important Update: {topic}\n\nPlease review the attached information and take necessary action.\n\nRegards,\nSystem"
        };
    }
}