using MediatR;
using GitHubFreshdeskIntegration.Domain.Entities;
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
            // Get GitHub User
            var gitHubUser = await _gitHubService.GetUserAsync(request.Username, cancellationToken);

            // Use the extension method to map fields to Freshdesk contact
            var freshdeskContact = gitHubUser.ToFreshdeskContact();

            // Check if contact already exists in Freshdesk
            var existingContact = await _freshdeskService.GetContactByEmailAsync(gitHubUser.Email, cancellationToken);
            if (existingContact != null)
            {
                // Update contact
                await _freshdeskService.UpdateContactAsync(existingContact.Id, freshdeskContact, cancellationToken);
            }
            else
            {
                // Create new contact
                await _freshdeskService.CreateContactAsync(freshdeskContact, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
