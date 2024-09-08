using GitHubFreshdeskIntegration.Domain.Entities;
using Refit;

namespace GitHubFreshdeskIntegration.Infrastructure.Interfaces
{
    public interface IGitHubApi
    {
        [Get("/users/{username}")]
        Task<GitHubUser> GetUserAsync(string username, CancellationToken cancellationToken);
    }
}
