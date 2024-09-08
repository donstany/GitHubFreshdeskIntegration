using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Refit;
using GitHubFreshdeskIntegration.Infrastructure.Services;
using GitHubFreshdeskIntegration.Infrastructure.Interfaces;

namespace GitHubFreshdeskIntegration.WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtKey = configuration["Jwt:SecretKey"];
            var key = Encoding.ASCII.GetBytes(jwtKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Set to true in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }

        public static void AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GitHubFreshdeskIntegration API", Version = "v1" });

                // Configure Swagger to use JWT Bearer Authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    In = ParameterLocation.Header,
                    Description = "Please enter token in the format: Bearer {your_token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            });
        }

        public static void AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
            });
        }

        public static void AddRefitClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<GitHubFreshdeskIntegrationSettings>(configuration.GetSection("GitHubFreshdeskIntegration"));

            var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            var freshdeskToken = Environment.GetEnvironmentVariable("FRESHDESK_TOKEN");

            if (string.IsNullOrEmpty(githubToken))
            {
                throw new InvalidOperationException("GitHub token is not set.");
            }

            if (string.IsNullOrEmpty(freshdeskToken))
            {
                throw new InvalidOperationException("Freshdesk token is not set.");
            }

            var settings = configuration.GetSection("GitHubFreshdeskIntegration").Get<GitHubFreshdeskIntegrationSettings>();

            // Configure GitHub Refit client with Bearer token
            services.AddRefitClient<IGitHubApi>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(settings.GitHubApiBaseUrl);
                    c.DefaultRequestHeaders.Add("User-Agent", "GitHubFreshdeskIntegration");
                    c.DefaultRequestHeaders.Add("Authorization", $"Bearer {githubToken}");
                });

            // Configure Freshdesk Refit client with Basic Auth
            services.AddRefitClient<IFreshdeskApi>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(settings.FreshdeskApiBaseUrl);
                    var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{freshdeskToken}:X"));
                    c.DefaultRequestHeaders.Add("Authorization", $"Basic {encodedToken}");
                });
        }
    }
}
