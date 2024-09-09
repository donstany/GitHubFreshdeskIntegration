using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Refit;
using GitHubFreshdeskIntegration.Infrastructure.Services;
using GitHubFreshdeskIntegration.Infrastructure.Interfaces;
using Polly;
using Polly.Extensions.Http;

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

            var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("PollyPolicies");

            // Retry Policy with Jitter
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(new Random().Next(0, 100)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        logger.LogWarning($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds due to {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                    });

            // Circuit Breaker Policy
            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30),
                    onBreak: (outcome, breakDelay) =>
                    {
                        logger.LogWarning($"Circuit breaker opened for {breakDelay.TotalSeconds} seconds due to {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                    },
                    onReset: () => logger.LogInformation("Circuit breaker reset."),
                    onHalfOpen: () => logger.LogInformation("Circuit breaker half-open; testing the external service."));

            // Timeout Policy
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10, onTimeoutAsync: (context, timespan, task) =>
            {
                logger.LogWarning($"Execution timed out after {timespan.TotalSeconds} seconds.");
                return Task.CompletedTask;
            });

            // Bulkhead Policy
            var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(
                maxParallelization: 10,
                maxQueuingActions: 20,
                onBulkheadRejectedAsync: context =>
                {
                    logger.LogWarning("Bulkhead rejection occurred.");
                    return Task.CompletedTask;
                });

            // Fallback Policy
            var fallbackPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                .FallbackAsync(
                    fallbackValue: new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent("{ \"message\": \"Service is currently unavailable. Please try again later.\" }")
                    },
                    onFallbackAsync: (result, context) =>
                    {
                        logger.LogWarning($"Fallback triggered due to {result.Exception?.Message ?? result.Result.StatusCode.ToString()}");
                        return Task.CompletedTask;
                    });

            // Combine policies into one policy wrap
            var resiliencePolicy = Policy.WrapAsync(fallbackPolicy, bulkheadPolicy, circuitBreakerPolicy, retryPolicy, timeoutPolicy);

            // Configure GitHub Refit client with Bearer token and resilience policies
            services.AddRefitClient<IGitHubApi>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(settings.GitHubApiBaseUrl);
                    c.DefaultRequestHeaders.Add("User-Agent", "GitHubFreshdeskIntegration");
                    c.DefaultRequestHeaders.Add("Authorization", $"Bearer {githubToken}");
                })
                .AddPolicyHandler(resiliencePolicy);

            // Configure Freshdesk Refit client with Basic Auth and resilience policies
            services.AddRefitClient<IFreshdeskApi>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(settings.FreshdeskApiBaseUrl);
                    var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{freshdeskToken}:X"));
                    c.DefaultRequestHeaders.Add("Authorization", $"Basic {encodedToken}");
                })
                .AddPolicyHandler(resiliencePolicy);
        }
    }
}
