using GitHubFreshdeskIntegration.Domain.Entities;

namespace GitHubFreshdeskIntegration.Application.Interfaces
{
    public interface IGitHubService
    {
        Task<GitHubUser> GetUserAsync(string username, CancellationToken cancellationToken);
    }
}
