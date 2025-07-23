using Common.SharedKernel.Application.Rpc;
using Trusts.Domain.Trusts;

namespace Trusts.IntegrationEvents.ObjectiveTrustValidation;

public sealed class ValidateObjectiveTrustConsumer(
    ITrustRepository repository) : IRpcHandler<ValidateObjectiveTrustRequest, ValidateObjectiveTrustResponse>
{

    public async Task<ValidateObjectiveTrustResponse> HandleAsync(
        ValidateObjectiveTrustRequest request,
        CancellationToken cancellationToken)
    {
        var trusts = await repository.GetByObjectiveIdAsync(request.ObjectiveId, cancellationToken);
        
        if (!trusts.Any())
        {
            return new ValidateObjectiveTrustResponse(true, false, false);
        }

        var hasBalance = trusts.Any(t => t.TotalBalance > 0);

        //si se está intentando inactivar y hay fideicomisos con saldo, no permitir
        if (request.RequestedStatus == "I" && hasBalance)
        {
            return new ValidateObjectiveTrustResponse(
                false,
                true,
                true);
        }

        return new ValidateObjectiveTrustResponse(true, true, hasBalance);
    }
}