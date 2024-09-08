using GitHubFreshdeskIntegration.Application.Features.Authentication.DTOs;
using MediatR;

namespace GitHubFreshdeskIntegration.Application.Features.Authentication.Commands
{
    public class LoginCommand : IRequest<TokenResponse>
    {
        public LoginModel Model { get; set; }

        public LoginCommand(LoginModel model)
        {
            Model = model;
        }
    }
}
