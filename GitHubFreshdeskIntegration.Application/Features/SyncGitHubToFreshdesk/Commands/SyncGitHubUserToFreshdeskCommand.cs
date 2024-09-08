using MediatR;

namespace GitHubFreshdeskIntegration.Application.Features.SyncGitHubToFreshdesk.Commands
{
    public class SyncGitHubUserToFreshdeskCommand : IRequest<Unit>
    {
        public string Username { get; set; }
        public string FreshdeskSubdomain { get; set; }

        public SyncGitHubUserToFreshdeskCommand() { }

        public SyncGitHubUserToFreshdeskCommand(string username, string freshdeskSubdomain)
        {
            Username = username;
            FreshdeskSubdomain = freshdeskSubdomain;
        }
    }
}
