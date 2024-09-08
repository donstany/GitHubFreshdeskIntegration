using FluentValidation;
using GitHubFreshdeskIntegration.Application.Features.Authentication.Commands;

namespace GitHubFreshdeskIntegration.Application.Features.Authentication.Validators
{
    public class LoginCommandValidator: AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Model.Username)
              .NotEmpty().WithMessage("Username cannot be empty.")
              .Length(1, 50).WithMessage("Username length must be between 1 and 50 characters.");

            RuleFor(x => x.Model.Password)
                .NotEmpty().WithMessage("Password cannot be empty.")
                .Length(1, 50).WithMessage("Freshdesk subdomain length must be between 1 and 50 characters.");
        }
    }
}
