using GitHubFreshdeskIntegration.Domain.Entities;

namespace GitHubFreshdeskIntegration.Application.Features.Interfaces
{
    public interface IGitHubService
    {
        Task<GitHubUser> GetUserAsync(string username, CancellationToken cancellationToken);
    }
}
