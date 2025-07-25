using Associate.Application.Abstractions;
using Associate.Domain.Activates;
using Associate.Domain.ConfigurationParameters;
using Associate.Integrations.Balances.AssociateBalancesById;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;
using Trusts.IntegrationEvents.GetBalances;
using Products.IntegrationEvents.AdditionalInformation;
using Customers.IntegrationEvents.PersonValidation;
using Common.SharedKernel.Application.Rpc;

namespace Associate.Application.Balances.AssociateBalancesById;

internal sealed class AssociateBalancesByIdHandler(
    IActivateRepository activateRepository,
    IConfigurationParameterRepository configurationParameterRepository,
    IRuleEvaluator<AssociateModuleMarker> ruleEvaluator,
    IRpcClient rpcClient)
    : IQueryHandler<AssociateBalancesByIdQuery, IReadOnlyCollection<AssociateBalanceWrapper>>
{
    private const string ValidationWorkflow = "Associate.BalancesById.Validation";
    private const string ContributionWorkflow = "Associate.BalancesById.ContributionValidation";

    public async Task<Result<IReadOnlyCollection<AssociateBalanceWrapper>>> Handle(
        AssociateBalancesByIdQuery request,
        CancellationToken cancellationToken)
    {
        var documentType = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.DocumentType,
            HomologScope.Of<AssociateBalancesByIdQuery>(q => q.DocumentType),
            cancellationToken);

        Activate? activation = null;
        if (documentType is not null)
            activation = await activateRepository.GetByIdTypeAndNumber(
                documentType.Uuid,
                request.Identification,
                cancellationToken);

        var validationContext = new
        {
            DocumentTypeProvided = !string.IsNullOrWhiteSpace(request.DocumentType),
            DocumentTypeExists = documentType is not null,
            IdentificationProvided = !string.IsNullOrWhiteSpace(request.Identification),
            ActivateExists = activation is not null
        };

        var (valid, _, errors) = await ruleEvaluator
            .EvaluateAsync(ValidationWorkflow, validationContext, cancellationToken);

        if (!valid)
        {
            var first = errors.First();
            return Result.Failure<IReadOnlyCollection<AssociateBalanceWrapper>>(
                Error.Validation(first.Code, first.Message));
        }

        var personValidation = await rpcClient.CallAsync<PersonDataRequestEvent, GetPersonValidationResponse>(
            new PersonDataRequestEvent(request.DocumentType, request.Identification),
            cancellationToken);

        if (!personValidation.IsValid)
            return Result.Failure<IReadOnlyCollection<AssociateBalanceWrapper>>(
                Error.Validation(personValidation.Code ?? string.Empty, personValidation.Message ?? string.Empty));

        var balancesRpc = await rpcClient.CallAsync<GetBalancesRequest, GetBalancesResponse>(
            new GetBalancesRequest(activation.ActivateId),
            cancellationToken);

        if (!balancesRpc.Succeeded)
            return Result.Failure<IReadOnlyCollection<AssociateBalanceWrapper>>(Error.Validation(balancesRpc.Code ?? string.Empty, balancesRpc.Message ?? string.Empty));

        var contributionContext = new { AffiliateHasContributions = balancesRpc.Balances.Any() };
        var (hasContributions, _, contributionErrors) = await ruleEvaluator
            .EvaluateAsync(ContributionWorkflow, contributionContext, cancellationToken);

        if (!hasContributions)
        {
            var first = contributionErrors.First();
            return Result.Failure<IReadOnlyCollection<AssociateBalanceWrapper>>(Error.Validation(first.Code, first.Message));
        }
        
        var objectivePortfolioPairs = balancesRpc.Balances
            .Select(b => (b.ObjectiveId, b.PortfolioId))
            .ToArray();

        var additionalInfoRpc = await rpcClient.CallAsync<GetAdditionalInformationRequest, GetAdditionalInformationResponse>(
            new GetAdditionalInformationRequest(activation.ActivateId, objectivePortfolioPairs),
            cancellationToken);

        if (!additionalInfoRpc.Succeeded)
            return Result.Failure<IReadOnlyCollection<AssociateBalanceWrapper>>(Error.Validation(additionalInfoRpc.Code ?? string.Empty, additionalInfoRpc.Message ?? string.Empty));

        var additionalInfoLookup = additionalInfoRpc.Items.ToDictionary(i => (i.ObjectiveId, i.PortfolioId));

        var items = balancesRpc.Balances.Select(b =>
        {
            additionalInfoLookup.TryGetValue((b.ObjectiveId, b.PortfolioId), out var additionalInfo);
            var item = new AssociateBalanceItem(
                (additionalInfo?.PortfolioCode ?? b.PortfolioId.ToString()),
                additionalInfo?.PortfolioName ?? string.Empty,
                b.ObjectiveId,
                additionalInfo?.ObjectiveName ?? string.Empty,
                additionalInfo?.AlternativeCode ?? string.Empty,
                additionalInfo?.AlternativeName ?? string.Empty,
                additionalInfo?.FundCode ?? string.Empty,
                additionalInfo?.FundName ?? string.Empty,
                b.TotalBalance.ToString("F2"),
                b.AvailableAmount.ToString("F2"));
            return new AssociateBalanceWrapper(item);
        }).ToList();

        return Result.Success<IReadOnlyCollection<AssociateBalanceWrapper>>(items);
    }
}
