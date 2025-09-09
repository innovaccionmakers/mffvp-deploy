using Common.SharedKernel.Application.Rpc;
using MediatR;
using Treasury.Integrations.BankAccounts.Queries;

namespace Treasury.IntegrationEvents.TreasuryMovements.ValidateExistByPortfolioAndAccountNumber;

public sealed class ValidateExistByPortfolioAndAccountNumberConsumer(ISender sender) : IRpcHandler<ValidateExistByPortfolioAndAccountNumberRequest, ValidateExistByPortfolioAndAccountNumberResponse>
{
    public async Task<ValidateExistByPortfolioAndAccountNumberResponse> HandleAsync(ValidateExistByPortfolioAndAccountNumberRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ExistByPortfolioAndAccountNumberQuery(request.PortfolioId, request.AccountNumber), ct);

        return result.IsSuccess
            ? new ValidateExistByPortfolioAndAccountNumberResponse(true, null, null, result.Value)
            : new ValidateExistByPortfolioAndAccountNumberResponse(false, result.Error!.Code, result.Error.Description, false);
    }
}
