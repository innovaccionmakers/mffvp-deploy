using Closing.Integrations.ProfitLosses.ProfitandLossLoad;
using Closing.Integrations.ProfitLosses.GetProfitandLoss;
using Common.SharedKernel.Domain;
using MediatR;

namespace MFFVP.Api.Application.Closing
{
    public interface IProfitLossService
    {
        Task<Result<bool>> LoadProfitLossAsync(
            ProfitandLossLoadCommand request,
            ISender sender);

        Task<Result<GetProfitandLossResponse>> GetProfitandLossAsync(
            GetProfitandLossQuery request,
            ISender sender);
    }
}