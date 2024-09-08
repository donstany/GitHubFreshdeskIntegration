using MediatR;
using GitHubFreshdeskIntegration.Application.Interfaces;
using GitHubFreshdeskIntegration.Application.Extensions;


namespace GitHubFreshdeskIntegration.Application.Features.SyncGitHubToFreshdesk.Commands
{
    public class SyncGitHubUserToFreshdeskHandler : IRequestHandler<SyncGitHubUserToFreshdeskCommand, Unit>
    {
        private readonly IGitHubService _gitHubService;
        private readonly IFreshdeskService _freshdeskService;

        public SyncGitHubUserToFreshdeskHandler(IGitHubService gitHubService, IFreshdeskService freshdeskService)
        {
            _gitHubService = gitHubService;
            _freshdeskService = freshdeskService;
        }

        public async Task<Unit> Handle(SyncGitHubUserToFreshdeskCommand request, CancellationToken cancellationToken)
        {
            var gitHubUser = await _gitHubService.GetUserAsync(request.Username, cancellationToken);

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
