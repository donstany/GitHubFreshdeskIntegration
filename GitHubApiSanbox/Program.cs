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
        public string Id { get; set; }
        public string AvatarUrl { get; set; }
        public string Url { get; set; }
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
                                client.DefaultRequestHeaders.Add("Authorization", ("Bearer " + githubToken)); // Personal Access Token from donstany profile
                            });
                })
                .Build();

            // Resolve the API client
            var gitHubApi = host.Services.GetRequiredService<IGitHubApi>();

            // Replace 'octocat' with the GitHub username you want to query
            string username = "octocat";

            try
            {
                Console.WriteLine("Fetching GitHub user information");
                var user = await gitHubApi.GetUserAsync(username);
                //Console.WriteLine($"User: {user.Login}");
                //Console.WriteLine($"ID: {user.Id}");
                //Console.WriteLine($"Avatar URL: {user.AvatarUrl}");
                //Console.WriteLine($"Profile URL: {user.Url}");
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
