using Common.SharedKernel.Domain;
using MediatR;

using Activations.Integrations.Affiliates;
using Activations.Integrations.Affiliates.CreateAffiliate;
using Activations.Integrations.Affiliates.DeleteAffiliate;
using Activations.Integrations.Affiliates.GetAffiliate;
using Activations.Integrations.Affiliates.GetAffiliates;
using Activations.Integrations.Affiliates.UpdateAffiliate;

using MFFVP.Api.Application.Activations;
using Activations.Integrations.Affiliates.CreateActivation;

namespace MFFVP.Api.Infrastructure.Activations
{
    public sealed class AffiliatesService : IAffiliatesService
    {
        public async Task<Result<IReadOnlyCollection<AffiliateResponse>>> GetAffiliatesAsync(ISender sender)
        {
            return await sender.Send(new GetAffiliatesQuery());
        }

        public async Task<Result<AffiliateResponse>> GetAffiliateAsync(int id, ISender sender)
        {
            return await sender.Send(new GetAffiliateQuery(id));
        }

        public async Task<Result<AffiliateResponse>> CreateAffiliateAsync(CreateActivationCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result<AffiliateResponse>> UpdateAffiliateAsync(UpdateAffiliateCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> DeleteAffiliateAsync(int id, ISender sender)
        {
            return await sender.Send(new DeleteAffiliateCommand(id));
        }
    }
}