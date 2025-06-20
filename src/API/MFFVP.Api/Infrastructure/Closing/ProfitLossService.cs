using Common.SharedKernel.Domain;
using MediatR;
using Closing.Integrations.ProfitLosses.ProfitandLossLoad;
using Closing.Integrations.ProfitLosses.GetProfitandLoss;
using MFFVP.Api.Application.Closing;

namespace MFFVP.Api.Infrastructure.Closing
{
    public sealed class ProfitLossService : IProfitLossService
    {
        public async Task<Result<bool>> LoadProfitLossAsync(
            ProfitandLossLoadCommand request,
            ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result<GetProfitandLossResponse>> GetProfitandLossAsync(
            GetProfitandLossQuery request,
            ISender sender)
        {
            return await sender.Send(request);
        }
    }
}