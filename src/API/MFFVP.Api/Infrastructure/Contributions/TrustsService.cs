using Common.SharedKernel.Domain;
using MediatR;

using Contributions.Integrations.Trusts;
using Contributions.Integrations.Trusts.CreateTrust;
using Contributions.Integrations.Trusts.DeleteTrust;
using Contributions.Integrations.Trusts.GetTrust;
using Contributions.Integrations.Trusts.GetTrusts;
using Contributions.Integrations.Trusts.UpdateTrust;

using MFFVP.Api.Application.Contributions;

namespace MFFVP.Api.Infrastructure.Contributions
{
    public sealed class TrustsService : ITrustsService
    {
        public async Task<Result<IReadOnlyCollection<TrustResponse>>> GetTrustsAsync(ISender sender)
        {
            return await sender.Send(new GetTrustsQuery());
        }

        public async Task<Result<TrustResponse>> GetTrustAsync(Guid id, ISender sender)
        {
            return await sender.Send(new GetTrustQuery(id));
        }

        public async Task<Result> CreateTrustAsync(CreateTrustCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> UpdateTrustAsync(UpdateTrustCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> DeleteTrustAsync(Guid id, ISender sender)
        {
            return await sender.Send(new DeleteTrustCommand(id));
        }
    }
}