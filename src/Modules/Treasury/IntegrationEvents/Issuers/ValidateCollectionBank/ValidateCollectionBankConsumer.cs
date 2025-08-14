using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Application.Rules;
using Treasury.Application.Abstractions;
using Treasury.Domain.Issuers;

namespace Treasury.IntegrationEvents.Issuers.ValidateCollectionBank;

public sealed class ValidateCollectionBankConsumer(
    IIssuerRepository issuerRepository,
    IRuleEvaluator<TreasuryModuleMarker> ruleEvaluator)
    : IRpcHandler<ValidateCollectionBankRequest, ValidateCollectionBankResponse>
{
    private const string Workflow = "Treasury.CollectionBank.Validation";

    public async Task<ValidateCollectionBankResponse> HandleAsync(
        ValidateCollectionBankRequest message,
        CancellationToken cancellationToken)
    {
        var issuer = await issuerRepository.GetByHomologatedCodeAsync(
            message.HomologatedCode,
            cancellationToken);

        var context = new
        {
            IssuerExists = issuer is not null,
            IsBank = issuer?.IsBank == true
        };

        var (isValid, _, errors) =
            await ruleEvaluator.EvaluateAsync(Workflow, context, cancellationToken);

        if (!isValid)
        {
            var error = errors.First();
            return new ValidateCollectionBankResponse(false, error.Code, error.Message, null);
        }

        return new ValidateCollectionBankResponse(true, null, null, issuer!.Id);
    }
}