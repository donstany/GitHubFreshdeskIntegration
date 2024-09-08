using GitHubFreshdeskIntegration.Application.Interfaces;
using GitHubFreshdeskIntegration.Domain.Entities;

namespace GitHubFreshdeskIntegration.Infrastructure.Services
{

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
