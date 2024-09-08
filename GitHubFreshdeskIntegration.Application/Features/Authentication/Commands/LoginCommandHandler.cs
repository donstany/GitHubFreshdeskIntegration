using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GitHubFreshdeskIntegration.Application.Features.Authentication.DTOs;


namespace GitHubFreshdeskIntegration.Application.Features.Authentication.Commands
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenResponse>
    {
        private readonly string _secretKey;

        public LoginCommandHandler(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:SecretKey"]; // Fetch from configuration
        }

        public Task<TokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Validate user credentials (replace with real user validation logic)
            if (request.Model.Username == "testuser" && request.Model.Password == "password")
            {
                var token = GenerateJwtToken(request.Model.Username);
                return Task.FromResult(new TokenResponse { Token = token });
            }

            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        private string GenerateJwtToken(string username)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username)
            };

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
