using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;

namespace GitHubApiSanbox
{

    public interface IGitHubApi
    {
        [Get("/users/{username}")]
        Task<GitHubUser> GetUserAsync(string username);
    }

    public class GitHubUser
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string TwitterUsername { get; set; }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Configure Refit
                    var githubToken = "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";
                    services.AddRefitClient<IGitHubApi>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri("https://api.github.com");
                                client.DefaultRequestHeaders.Add("User-Agent", "ConsoleApp");
                                client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + githubToken)); // Personal Access Token from donstany profile valid till end of October 2024
                            });
                })
                .Build();

            // Resolve the API client
            var gitHubApi = host.Services.GetRequiredService<IGitHubApi>();

            // Replace 'octocat' with the GitHub username you want to query
            string username = "donstany";

            try
            {
                Console.WriteLine("Fetching GitHub user information");
                var user = await gitHubApi.GetUserAsync(username);
                Console.WriteLine($"User: {user.Email}");
                Console.WriteLine($"Login: {user.Login}");
                Console.WriteLine($"Name: {user.Name}");
                Console.WriteLine($"Phone: {user.Phone}");
                Console.WriteLine($"TwitterUsername URL: {user.TwitterUsername}");
                ;
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
                if (ex.Content != null)
                {
                    Console.WriteLine("Error Response: " + ex.Content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }
    }

}
