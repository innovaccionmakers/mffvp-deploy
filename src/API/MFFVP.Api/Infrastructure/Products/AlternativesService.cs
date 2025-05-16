
using Common.SharedKernel.Domain;
using MediatR;

using Products.Integrations.Alternatives;
using Products.Integrations.Alternatives.CreateAlternative;
using Products.Integrations.Alternatives.DeleteAlternative;
using Products.Integrations.Alternatives.GetAlternative;
using Products.Integrations.Alternatives.GetAlternatives;
using Products.Integrations.Alternatives.UpdateAlternative;

using MFFVP.Api.Application.Products;

namespace MFFVP.Api.Infrastructure.Products
{
    public sealed class AlternativesService : IAlternativesService
    {
        public async Task<Result<IReadOnlyCollection<AlternativeResponse>>> GetAlternativesAsync(ISender sender)
        {
            return await sender.Send(new GetAlternativesQuery());
        }

        public async Task<Result<AlternativeResponse>> GetAlternativeAsync(long alternativeId, ISender sender)
        {
            return await sender.Send(new GetAlternativeQuery(alternativeId));
        }

        public async Task<Result> CreateAlternativeAsync(CreateAlternativeCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> UpdateAlternativeAsync(UpdateAlternativeCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> DeleteAlternativeAsync(long alternativeId, ISender sender)
        {
            return await sender.Send(new DeleteAlternativeCommand(alternativeId));
        }
    }
}