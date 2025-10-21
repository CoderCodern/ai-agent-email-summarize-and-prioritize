using OpenAIConsole.Models;
using System.Text.Json;
using OpenAIConsole.Interfaces;

namespace OpenAIConsole.Services
{
    public class EmailFileService : IEmailFileService
    {
        public async Task<List<Email>> LoadEmailsAsync(string path)
        {
            if (!File.Exists(path))
                return new List<Email>();
            var json = await File.ReadAllTextAsync(path);
            var collection = JsonSerializer.Deserialize<EmailCollection>(json);
            return collection?.Emails ?? new List<Email>();
        }

        public async Task SaveEmailsAsync(string path, List<Email> emails)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            var json = JsonSerializer.Serialize(new EmailCollection { Emails = emails }, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
        }
    }
}
