using System.Text.Json;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.External;
using Common.SharedKernel.Application.Rules;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.ClientOperations;
using Operations.Domain.Banks;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Operations.Integrations.Contributions;
using Operations.Integrations.Contributions.CreateContribution;
using Operations.Domain.ConfigurationParameters;
using Common.SharedKernel.Application.Attributes;

namespace Operations.Application.Contributions.CreateContribution;

internal sealed class CreateContributionCommandHandler(
    IContributionCatalogResolver catalogResolver,
    IActivateLocator activateLocator,
    IContributionRemoteValidator remoteValidator,
    IPersonValidator personValidator,
    ITaxCalculator taxCalculator,
    IClientOperationRepository clientOperationRepository,
    IAuxiliaryInformationRepository auxiliaryInformationRepository,
    IBankRepository bankRepository,
    IConfigurationParameterRepository configurationParameterRepository,
    IRuleEvaluator<OperationsModuleMarker> ruleEvaluator,
    IUnitOfWork unitOfWork,
    ITrustCreator trustCreator)
    : ICommandHandler<CreateContributionCommand, ContributionResponse>
{
    private const string Flow = "Operations.Contribution.Validation";
    private const string RequiredFieldsFlow = "Operations.Contribution.RequiredFields";

    public async Task<Result<ContributionResponse>> Handle(CreateContributionCommand command,
        CancellationToken cancellationToken)
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
            return Result.Failure<ContributionResponse>(
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
            return Result.Failure<ContributionResponse>(actRes.Error!);

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
                PortfolioInitialMinimumAmount: 0));
        }

        if (!remoteRes.IsSuccess)
            return Result.Failure<ContributionResponse>(remoteRes.Error!);

        var catalogs = await catalogResolver.ResolveAsync(command, cancellationToken);

        var certifiedValid = string.IsNullOrWhiteSpace(command.CertifiedContribution) ||
                             command.CertifiedContribution.Trim().ToUpperInvariant() is "SI" or "NO";

        var firstContribution = affiliateFound && !await clientOperationRepository.ExistsContributionAsync(
            remoteRes.Value.AffiliateId,
            remoteRes.Value.ObjectiveId,
            remoteRes.Value.PortfolioId,
            cancellationToken);

        var bank = await bankRepository.FindByHomologatedCodeAsync(
            command.CollectionBank,
            cancellationToken);



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
            CollectionBankExists = bank is not null,
            IsFirstContribution = firstContribution,
            PortfolioInitialMinimumAmount = remoteRes.Value.PortfolioInitialMinimumAmount,
            Amount = command.Amount,
            CertifiedContributionProvided = !string.IsNullOrWhiteSpace(command.CertifiedContribution),
            CertifiedContributionValid = certifiedValid,
            SubtypeExists = catalogs.Subtype is not null,
            CategoryIsContribution = catalogs.SubtypeCategoryCfg?.Name == "Aporte"
        };



        var (ok, _, errs) = await ruleEvaluator.EvaluateAsync(Flow, contextValidation, cancellationToken);
        if (!ok)
        {
            var e = errs.First();
            return Result.Failure<ContributionResponse>(Error.Validation(e.Code, e.Message));
        }

        if (affiliateFound)
        {
            var personResult = await personValidator.ValidateAsync(command.TypeId, command.Identification, cancellationToken);
            if (!personResult.IsSuccess)
                return Result.Failure<ContributionResponse>(personResult.Error!);
        }

        var isCertified = command.CertifiedContribution?.Trim().ToUpperInvariant() == "SI";

        TaxResult tax = default;
        if (affiliateFound)
        {
            tax = await taxCalculator.ComputeAsync(actRes.Value.Item3, isCertified, command.Amount,
                cancellationToken);
        }
        else
        {
            tax = new TaxResult(
                TaxConditionId: 0,
                CertificationStatusId: 0,
                WithheldAmount: 0m,
                TaxConditionName: string.Empty);
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var operation = ClientOperation.Create(
            DateTime.UtcNow,
            remoteRes.Value.AffiliateId,
            remoteRes.Value.ObjectiveId,
            remoteRes.Value.PortfolioId,
            command.Amount,
            DateTime.SpecifyKind(command.ExecutionDate, DateTimeKind.Utc),
            catalogs.Subtype?.SubtransactionTypeId ?? 0).Value;
        clientOperationRepository.Insert(operation);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var aux = AuxiliaryInformation.Create(
            operation.ClientOperationId,
            catalogs.Source!.OriginId,
            catalogs.CollectionMethod!.ConfigurationParameterId,
            catalogs.PaymentMethod!.ConfigurationParameterId,
            int.TryParse(command.CollectionAccount, out var acc) ? acc : 0,
            command.PaymentMethodDetail ?? JsonDocument.Parse("{}"),
            tax.CertificationStatusId,
            tax.TaxConditionId,
            0,
            command.VerifiableMedium ?? JsonDocument.Parse("{}"),
            bank?.BankId ?? 0,
            DateTime.SpecifyKind(command.DepositDate, DateTimeKind.Utc),
            command.SalesUser,
            catalogs.OriginModality!.ConfigurationParameterId,
            0,
            catalogs.Channel?.ChannelId ?? 0,
            int.TryParse(command.User, out var uid) ? uid : 0).Value;
        auxiliaryInformationRepository.Insert(aux);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var trustRes = await trustCreator.CreateAsync(
            new TrustCreationDto(
                remoteRes.Value.AffiliateId,
                operation.ClientOperationId,
                DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                remoteRes.Value.ObjectiveId,
                remoteRes.Value.PortfolioId,
                command.Amount,
                0,
                command.Amount,
                0m,
                tax.TaxConditionId,
                tax.WithheldAmount,
                0m,
                command.Amount),
            cancellationToken);
        if (!trustRes.IsSuccess)
            return Result.Failure<ContributionResponse>(trustRes.Error!);

        await transaction.CommitAsync(cancellationToken);

        var resp = new ContributionResponse(
            operation.ClientOperationId,
            remoteRes.Value.PortfolioId.ToString(),
            remoteRes.Value.PortfolioName,
            tax.TaxConditionName,
            tax.WithheldAmount);

        return Result.Success(resp, "Transacción causada Exitosamente");
    }


}