using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation.AspNetCore;
using GitHubFreshdeskIntegration.Application.Features.SyncGitHubToFreshdesk.Validators;
using GitHubFreshdeskIntegration.Infrastructure.Services;
using GitHubFreshdeskIntegration.WebAPI.Middleware;
using Refit;
using System.Reflection;
using FluentValidation;
using GitHubFreshdeskIntegration.Application.Interfaces;
using GitHubFreshdeskIntegration.Application.Features.Authentication.Commands;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:SecretKey"]; // Ensure this is set in your appsettings.json
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
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

// Register MediatR services
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<LoginCommandHandler>());

// Register FluentValidation services
builder.Services.AddValidatorsFromAssemblyContaining<SyncGitHubUserToFreshdeskCommandValidator>();
builder.Services.AddFluentValidationAutoValidation(); // Automatically validate model state
builder.Services.AddFluentValidationClientsideAdapters(); // Add client-side validation adapters if needed

// Load tokens from environment variables
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

// Add services to the container.
builder.Services.AddControllers();

// Configure GitHub Refit client with Bearer token
builder.Services.AddRefitClient<IGitHubApi>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri("https://api.github.com");
        c.DefaultRequestHeaders.Add("User-Agent", "GitHubFreshdeskIntegration");
        c.DefaultRequestHeaders.Add("Authorization", $"Bearer {githubToken}");
    });

// Configure Freshdesk Refit client with Basic Auth
builder.Services.AddRefitClient<IFreshdeskApi>()
    .ConfigureHttpClient(c =>
    {
        var freshdeskSubdomain = builder.Configuration["Freshdesk:Subdomain"];
        c.BaseAddress = new Uri($"https://{freshdeskSubdomain}.freshdesk.com/api/v2");
        var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{freshdeskToken}:X"));
        c.DefaultRequestHeaders.Add("Authorization", $"Basic {encodedToken}");
    });

builder.Services.AddTransient<IGitHubService, GitHubService>();
builder.Services.AddTransient<IFreshdeskService, FreshdeskService>();

builder.Services.AddLogging();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
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

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<GlobalExceptionMiddleware>();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GitHubFreshdeskIntegration API V1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at app root
});

app.UseHttpsRedirection();

// Add authentication middleware
//app.UseAuthentication();
//app.UseAuthorization();

// Apply CORS policy
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
