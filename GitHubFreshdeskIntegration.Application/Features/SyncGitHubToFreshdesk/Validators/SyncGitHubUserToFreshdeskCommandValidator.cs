using FluentValidation;
using GitHubFreshdeskIntegration.Application.Features.SyncGitHubToFreshdesk.Commands;

namespace GitHubFreshdeskIntegration.Application.Features.SyncGitHubToFreshdesk.Validators
{
    public class SyncGitHubUserToFreshdeskCommandValidator : AbstractValidator<SyncGitHubUserToFreshdeskCommand>
    {
        public SyncGitHubUserToFreshdeskCommandValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username cannot be empty.")
                .Length(1, 50).WithMessage("Username length must be between 1 and 50 characters.");

            RuleFor(x => x.FreshdeskSubdomain)
                .NotEmpty().WithMessage("Freshdesk subdomain cannot be empty.")
                .Length(1, 100).WithMessage("Freshdesk subdomain length must be between 1 and 100 characters.");
        }
    }
}
