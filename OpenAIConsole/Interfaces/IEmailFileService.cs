using OpenAIConsole.Models;

namespace OpenAIConsole.Interfaces;

public interface IEmailFileService
{
    Task<List<Email>> LoadEmailsAsync(string path);
    Task SaveEmailsAsync(string path, List<Email> emails);
}
