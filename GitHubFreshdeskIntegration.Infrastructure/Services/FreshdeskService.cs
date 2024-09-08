using GitHubFreshdeskIntegration.Application.Features.Interfaces;
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

    public class FreshdeskService : IFreshdeskService
    {
        private readonly IFreshdeskApi _freshdeskApi;

        public FreshdeskService(IFreshdeskApi freshdeskApi)
        {
            _freshdeskApi = freshdeskApi;
        }

        public async Task<FreshdeskContact> GetContactByEmailAsync(string email, CancellationToken cancellationToken)
        {
            var response = await _freshdeskApi.GetContactByEmailAsync(email, cancellationToken);
            return response.Content?.Count > 0 ? response.Content[0] : null;
        }

        public async Task<FreshdeskContact> CreateContactAsync(FreshdeskContact contact, CancellationToken cancellationToken)
        {
            return await _freshdeskApi.CreateContactAsync(contact, cancellationToken);
        }

        public async Task UpdateContactAsync(long id, FreshdeskContact contact, CancellationToken cancellationToken)
        {
            await _freshdeskApi.UpdateContactAsync(id, contact, cancellationToken);
        }
    }
}
