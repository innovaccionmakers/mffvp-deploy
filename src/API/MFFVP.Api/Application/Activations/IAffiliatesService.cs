using Activations.Integrations.Affiliates;
using Activations.Integrations.Affiliates.CreateActivation;
using Activations.Integrations.Affiliates.UpdateAffiliate;
using Common.SharedKernel.Domain;
using MediatR;

namespace MFFVP.Api.Application.Activations;

public interface IAffiliatesService
{
    Task<Result<IReadOnlyCollection<AffiliateResponse>>> GetAffiliatesAsync(ISender sender);
    Task<Result<AffiliateResponse>> GetAffiliateAsync(int id, ISender sender);
    Task<Result<AffiliateResponse>> CreateAffiliateAsync(CreateActivationCommand request, ISender sender);
    Task<Result<AffiliateResponse>> UpdateAffiliateAsync(UpdateAffiliateCommand request, ISender sender);
    Task<Result> DeleteAffiliateAsync(int id, ISender sender);
}