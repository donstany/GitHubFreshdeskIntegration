using GitHubFreshdeskIntegration.Application.Interfaces;
using GitHubFreshdeskIntegration.Domain.Entities;
using Refit;

namespace GitHubFreshdeskIntegration.Infrastructure.Services
{
    //TODO excreact interface in separate file
    public interface IGitHubApi
    {
        [Get("/users/{username}")]
        Task<GitHubUser> GetUserAsync(string username, CancellationToken cancellationToken);
    }

    public class GitHubService : IGitHubService
    {
        private readonly IGitHubApi _gitHubApi;

        public GitHubService(IGitHubApi gitHubApi)
        {
            _gitHubApi = gitHubApi;
        }

        public async Task<GitHubUser> GetUserAsync(string username, CancellationToken cancellationToken)
        {
            return await _gitHubApi.GetUserAsync(username, cancellationToken);
        }
    }
}
