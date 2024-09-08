using GitHubFreshdeskIntegration.Domain.Entities;
using Refit;

namespace GitHubFreshdeskIntegration.Infrastructure.Services
{
    public interface IFreshdeskApi
    {
        [Get("/contacts?email={email}")]
        Task<ApiResponse<List<FreshdeskContact>>> GetContactByEmailAsync(string email, CancellationToken cancellationToken);

        [Post("/contacts")]
        Task<FreshdeskContact> CreateContactAsync([Body] FreshdeskContact contact, CancellationToken cancellationToken);

        [Put("/contacts/{id}")]
        Task UpdateContactAsync(long id, [Body] FreshdeskContact contact, CancellationToken cancellationToken);
    }
}
