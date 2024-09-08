using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;


namespace FreshdeskApiSandbox
{
    public interface IFreshdeskApi
    {
        [Put("/api/v2/tickets/{ticketId}")]
        Task UpdateTicketAsync(long ticketId, [Body] TicketUpdateRequest request);
    }

    public class TicketUpdateRequest
    {
        public int Priority { get; set; }
        public int Status { get; set; }
        public string[] Tags { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Configure Refit
                    services.AddRefitClient<IFreshdeskApi>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri("https://YOUR_DOMAIN.freshdesk.com");
                                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("YOUR_API_KEY:X")));
                            });
                })
                .Build();

            // Resolve the API client
            var freshdeskApi = host.Services.GetRequiredService<IFreshdeskApi>();

            var updateRequest = new TicketUpdateRequest
            {
                Priority = 1,
                Status = 2,
                Tags = new[] { "csv1", "csv2" }
            };

            try
            {
                Console.WriteLine("Submitting Request");
                await freshdeskApi.UpdateTicketAsync(1, updateRequest);
                Console.WriteLine("Request successfully submitted.");
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
