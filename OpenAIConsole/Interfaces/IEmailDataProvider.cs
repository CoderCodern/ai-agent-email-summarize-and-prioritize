using OpenAIConsole.Models;

namespace OpenAIConsole.Interfaces;

public interface IEmailDataProvider
{
    Task<List<Email>> GetEmailsAsync();
    Task SaveEmailsAsync(List<Email> emails);
}
