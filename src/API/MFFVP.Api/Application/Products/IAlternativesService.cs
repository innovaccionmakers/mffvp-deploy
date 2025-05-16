using Common.SharedKernel.Domain;
using MediatR;

using Products.Integrations.Alternatives;
using Products.Integrations.Alternatives.CreateAlternative;
using Products.Integrations.Alternatives.UpdateAlternative;

namespace MFFVP.Api.Application.Products
{
    public interface IAlternativesService
    {
        Task<Result<IReadOnlyCollection<AlternativeResponse>>> GetAlternativesAsync(ISender sender);
        Task<Result<AlternativeResponse>> GetAlternativeAsync(long alternativeId, ISender sender);
        Task<Result> CreateAlternativeAsync(CreateAlternativeCommand request, ISender sender);
        Task<Result> UpdateAlternativeAsync(UpdateAlternativeCommand request, ISender sender);
        Task<Result> DeleteAlternativeAsync(long alternativeId, ISender sender);
    }
}