using GitHubFreshdeskIntegration.Application.Interfaces;
using GitHubFreshdeskIntegration.Domain.Entities;
using GitHubFreshdeskIntegration.Infrastructure.Interfaces;

namespace GitHubFreshdeskIntegration.Infrastructure.Services
{

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
