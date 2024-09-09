using MediatR;
using GitHubFreshdeskIntegration.Application.Interfaces;
using GitHubFreshdeskIntegration.Application.Extensions;
using Microsoft.Extensions.Logging;


namespace GitHubFreshdeskIntegration.Application.Features.SyncGitHubToFreshdesk.Commands
{
    public class SyncGitHubUserToFreshdeskHandler : IRequestHandler<SyncGitHubUserToFreshdeskCommand, Unit>
    {
        private readonly IGitHubService _gitHubService;
        private readonly IFreshdeskService _freshdeskService;
        private readonly ILogger<SyncGitHubUserToFreshdeskHandler> _logger;

        public SyncGitHubUserToFreshdeskHandler(IGitHubService gitHubService, IFreshdeskService freshdeskService, ILogger<SyncGitHubUserToFreshdeskHandler> logger)
        {
            _gitHubService = gitHubService;
            _freshdeskService = freshdeskService;
            _logger = logger;
        }

        public async Task<Unit> Handle(SyncGitHubUserToFreshdeskCommand request, CancellationToken cancellationToken)
        {
            var gitHubUser = await _gitHubService.GetUserAsync(request.Username, cancellationToken);
            if (gitHubUser == null)
            {
                _logger.LogWarning($"GitHub user '{request.Username}' not found. Not any data for synchronizing!");
                return Unit.Value;
            }

            var freshdeskContact = gitHubUser.ToFreshdeskContact();

            var existingContactInFreshdesk = await _freshdeskService.GetContactByEmailAsync(gitHubUser.Email, cancellationToken);

            if (existingContactInFreshdesk != null)
            {
                await _freshdeskService.UpdateContactAsync(existingContactInFreshdesk.Id, freshdeskContact, cancellationToken);
            }
            else
            {
                await _freshdeskService.CreateContactAsync(freshdeskContact, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
