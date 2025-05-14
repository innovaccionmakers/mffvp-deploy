
using Common.SharedKernel.Domain;
using MediatR;

using Trusts.Integrations.Trusts;
using Trusts.Integrations.Trusts.CreateTrust;
using Trusts.Integrations.Trusts.DeleteTrust;
using Trusts.Integrations.Trusts.GetTrust;
using Trusts.Integrations.Trusts.GetTrusts;
using Trusts.Integrations.Trusts.UpdateTrust;

using MFFVP.Api.Application.Trusts;

namespace MFFVP.Api.Infrastructure.Trusts
{
    public sealed class TrustsService : ITrustsService
    {
        public async Task<Result<IReadOnlyCollection<TrustResponse>>> GetTrustsAsync(ISender sender)
        {
            return await sender.Send(new GetTrustsQuery());
        }

        public async Task<Result<TrustResponse>> GetTrustAsync(long trustId, ISender sender)
        {
            return await sender.Send(new GetTrustQuery(trustId));
        }

        public async Task<Result> CreateTrustAsync(CreateTrustCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> UpdateTrustAsync(UpdateTrustCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> DeleteTrustAsync(long trustId, ISender sender)
        {
            return await sender.Send(new DeleteTrustCommand(trustId));
        }
    }
}