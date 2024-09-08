using GitHubFreshdeskIntegration.Application.Features.Interfaces;
using GitHubFreshdeskIntegration.Domain.Entities;
using Refit;

namespace GitHubFreshdeskIntegration.Infrastructure.Services
{
    public interface IFreshdeskApi
    {
        [Get("/contacts?email={email}")]
        Task<ApiResponse<List<FreshdeskContact>>> GetContactByEmailAsync(string email);

        [Post("/contacts")]
        Task<FreshdeskContact> CreateContactAsync([Body] FreshdeskContact contact);

        [Put("/contacts/{id}")]
        Task UpdateContactAsync(long id, [Body] FreshdeskContact contact);
    }

    public class FreshdeskService : IFreshdeskService
    {
        private readonly IFreshdeskApi _freshdeskApi;

        public FreshdeskService(IFreshdeskApi freshdeskApi)
        {
            _freshdeskApi = freshdeskApi;
        }

        public async Task<FreshdeskContact> GetContactByEmailAsync(string email)
        {
            var response = await _freshdeskApi.GetContactByEmailAsync(email);
            return response.Content?.Count > 0 ? response.Content[0] : null;
        }

        public async Task<FreshdeskContact> CreateContactAsync(FreshdeskContact contact)
        {
            return await _freshdeskApi.CreateContactAsync(contact);
        }

        public async Task UpdateContactAsync(long id, FreshdeskContact contact)
        {
            await _freshdeskApi.UpdateContactAsync(id, contact);
        }
    }
}
