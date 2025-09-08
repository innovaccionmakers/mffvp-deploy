using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Operations.Application.Abstractions;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.External;
using Operations.Application.Abstractions.Services.Prevalidation;
using Operations.Domain.ClientOperations;
using Operations.Domain.ConfigurationParameters;
using Operations.Integrations.Contributions.CreateContribution;

namespace Operations.Application.Contributions.Prevalidation;

public sealed class Prevalidate(
    IContributionCatalogResolver catalogResolver,
    IActivateLocator activateLocator,
    IContributionRemoteValidator remoteValidator,
    IPersonValidator personValidator,
    IClientOperationRepository clientOperationRepository,
    ICollectionBankValidator collectionBankValidator,
    IConfigurationParameterRepository configurationParameterRepository,
    IRuleEvaluator<OperationsModuleMarker> ruleEvaluator)
    : IPrevalidate
{
    private const string Flow = "Operations.Contribution.Validation";
    private const string RequiredFieldsFlow = "Operations.Contribution.RequiredFields";

    public async Task<Result<PrevalidationResult>> ValidateAsync(CreateContributionCommand command, CancellationToken cancellationToken)
    {
        var requiredCtx = new
        {
            command.TypeId,
            command.Identification,
            command.ObjectiveId,
            command.Amount,
            command.Origin,
            command.OriginModality,
            command.CollectionMethod,
            command.PaymentMethod,
            command.CollectionBank,
            command.CollectionAccount,
            command.DepositDate,
            command.ExecutionDate,
            command.SalesUser,
            command.Channel,
            command.User
        };

        var (requiredOk, _, requiredErrs) = await ruleEvaluator.EvaluateAsync(
            RequiredFieldsFlow, requiredCtx, cancellationToken);

        if (!requiredOk)
        {
            var e = requiredErrs.First();
            return Result.Failure<PrevalidationResult>(
                Error.Validation(e.Code, e.Message));
        }

        var documentType = await configurationParameterRepository.GetByCodeAndScopeAsync(
            command.TypeId,
            HomologScope.Of<CreateContributionCommand>(c => c.TypeId),
            cancellationToken);

        var documentTypeExists = documentType is not null;

        var actRes = documentTypeExists
            ? await activateLocator.FindAsync(command.TypeId, command.Identification, cancellationToken)
            : Result.Success((false, 0, false));

        if (!actRes.IsSuccess)
            return Result.Failure<PrevalidationResult>(actRes.Error!);

        var affiliateFound = actRes.Value.Item1;

        Result<ContributionRemoteData> remoteRes;
        if (affiliateFound)
        {
            remoteRes = await remoteValidator.ValidateAsync(
                actRes.Value.Item2,
                command.ObjectiveId,
                command.PortfolioId,
                command.DepositDate,
                command.ExecutionDate,
                command.Amount,
                cancellationToken);
        }
        else
        {
            remoteRes = Result.Success(new ContributionRemoteData(
                AffiliateId: 0,
                ObjectiveId: command.ObjectiveId,
                PortfolioId: 0,
                PortfolioName: string.Empty,
                PortfolioInitialMinimumAmount: 0,
                PortfolioAdditionalMinimumAmount: 0));
        }

        if (!remoteRes.IsSuccess)
            return Result.Failure<PrevalidationResult>(remoteRes.Error!);

        var catalogs = await catalogResolver.ResolveAsync(command, cancellationToken);

        var certifiedValid = string.IsNullOrWhiteSpace(command.CertifiedContribution) ||
                             command.CertifiedContribution.Trim().ToUpperInvariant() is "SI" or "NO";

        var firstContribution = affiliateFound && !await clientOperationRepository.ExistsContributionAsync(
            remoteRes.Value.AffiliateId,
            remoteRes.Value.ObjectiveId,
            remoteRes.Value.PortfolioId,
            cancellationToken);

        var bankResult = await collectionBankValidator.ValidateAsync(
            command.CollectionBank,
            cancellationToken);

        long portfolioId = long.TryParse(command.PortfolioId, out var n) ? n : 0;

        var existBankAccount = await collectionBankValidator.ValidateExistByPortfolioAndAccountNumberAsync(portfolioId, command.CollectionAccount, cancellationToken);


        if (!bankResult.IsSuccess)
            return Result.Failure<PrevalidationResult>(bankResult.Error!);

        var bankId = bankResult.Value;

        var contextValidation = new
        {
            DocumentTypeExists = documentTypeExists,
            AffiliateFound = affiliateFound,
            ContributionSourceExists = catalogs.Source is not null,
            ContributionSourceActive = catalogs.Source?.Status == Status.Active,
            ContributionSourceRequiresCertification = catalogs.Source?.RequiresCertification ?? false,
            OriginModalityExists = catalogs.OriginModality is not null,
            OriginModalityActive = catalogs.OriginModality?.Status ?? false,
            CollectionMethodExists = catalogs.CollectionMethod is not null,
            CollectionMethodActive = catalogs.CollectionMethod?.Status ?? false,
            PaymentMethodExists = catalogs.PaymentMethod is not null,
            PaymentMethodActive = catalogs.PaymentMethod?.Status ?? false,
            ChannelExists = catalogs.Channel is not null,
            IsFirstContribution = firstContribution,
            PortfolioInitialMinimumAmount = remoteRes.Value.PortfolioInitialMinimumAmount,
            PortfolioAdditionalMinimumAmount = remoteRes.Value.PortfolioAdditionalMinimumAmount,
            Amount = command.Amount,
            CertifiedContributionProvided = !string.IsNullOrWhiteSpace(command.CertifiedContribution),
            CertifiedContributionValid = certifiedValid,
            SubtypeExists = catalogs.Subtype is not null,
            CategoryIsContribution = catalogs.SubtypeCategory?.Name == "Aporte",
            BankAccountExists = existBankAccount.Value
        };

        var (ok, _, errs) = await ruleEvaluator.EvaluateAsync(Flow, contextValidation, cancellationToken);
        if (!ok)
        {
            var e = errs.First();
            return Result.Failure<PrevalidationResult>(Error.Validation(e.Code, e.Message));
        }

        if (affiliateFound)
        {
            var personResult = await personValidator.ValidateAsync(command.TypeId, command.Identification, cancellationToken);
            if (!personResult.IsSuccess)
                return Result.Failure<PrevalidationResult>(personResult.Error!);
        }

        var result = new PrevalidationResult(
            actRes.Value,
            remoteRes.Value,
            catalogs,
            bankId,
            firstContribution,
            documentTypeExists,
            affiliateFound
        );

        return Result.Success(result);
    }
}