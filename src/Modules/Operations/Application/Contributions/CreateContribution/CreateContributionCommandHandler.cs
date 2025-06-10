using System.Text.Json;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.External;
using Operations.Application.Abstractions.Rules;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.ClientOperations;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Operations.Integrations.Contributions;
using Operations.Integrations.Contributions.CreateContribution;

namespace Operations.Application.Contributions.CreateContribution;

internal sealed class CreateContributionCommandHandler(
    IContributionCatalogResolver catalogResolver,
    IActivateLocator activateLocator,
    IContributionRemoteValidator remoteValidator,
    IPersonValidator personValidator,
    ITaxCalculator taxCalculator,
    IClientOperationRepository clientOperationRepository,
    IAuxiliaryInformationRepository auxiliaryInformationRepository,
    IRuleEvaluator<OperationsModuleMarker> ruleEvaluator,
    IUnitOfWork unitOfWork,
    ITrustCreator trustCreator)
    : ICommandHandler<CreateContributionCommand, ContributionResponse>
{
    private const string Flow = "Operations.Contribution.Validation";

    public async Task<Result<ContributionResponse>> Handle(CreateContributionCommand command,
        CancellationToken cancellationToken)
    {
        var catalogs = await catalogResolver.ResolveAsync(command, cancellationToken);

        var actRes =
            await activateLocator.FindAsync(command.TypeId, command.Identification, cancellationToken);
        if (!actRes.IsSuccess)
            return Result.Failure<ContributionResponse>(actRes.Error!);

        var remoteRes = await remoteValidator.ValidateAsync(
            actRes.Value.ActivateId,
            command.ObjectiveId,
            command.PortfolioId,
            command.DepositDate,
            command.ExecutionDate,
            command.Amount,
            cancellationToken);
        if (!remoteRes.IsSuccess)
            return Result.Failure<ContributionResponse>(remoteRes.Error!);

        var certifiedValid = string.IsNullOrWhiteSpace(command.CertifiedContribution) ||
                             command.CertifiedContribution.Trim().ToUpperInvariant() is "SI" or "NO";

        var firstContribution = !await clientOperationRepository.ExistsContributionAsync(
            remoteRes.Value.AffiliateId,
            remoteRes.Value.ObjectiveId,
            remoteRes.Value.PortfolioId,
            cancellationToken);

        var contextValidation = new
        {
            ContributionSource = new
            {
                Exists = catalogs.Source is not null,
                Active = catalogs.Source?.Status.Equals("A", StringComparison.OrdinalIgnoreCase) == true,
                RequiresCertification = catalogs.Source?.RequiresCertification ?? false
            },
            OriginModality = BuildCtx(catalogs.OriginModality),
            CollectionMethod = BuildCtx(catalogs.CollectionMethod),
            PaymentMethod = BuildCtx(catalogs.PaymentMethod),
            Channel = new { Exists = catalogs.Channel is not null },
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

        var personResult =
            await personValidator.ValidateAsync(command.TypeId, command.Identification, cancellationToken);
        if (!personResult.IsSuccess)
            return Result.Failure<ContributionResponse>(personResult.Error!);

        var isCertified = command.CertifiedContribution?.Trim().ToUpperInvariant() == "SI";
        var tax = await taxCalculator.ComputeAsync(actRes.Value.IsPensioner, isCertified, command.Amount,
            cancellationToken);

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var operation = ClientOperation.Create(
            DateTime.UtcNow,
            remoteRes.Value.AffiliateId,
            remoteRes.Value.ObjectiveId,
            remoteRes.Value.PortfolioId,
            command.Amount,
            command.ExecutionDate,
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
            command.CollectionBank,
            command.DepositDate,
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
                DateTime.UtcNow,
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
            command.PortfolioId,
            remoteRes.Value.PortfolioName,
            tax.TaxConditionName,
            tax.WithheldAmount);

        return Result.Success(resp, "Transacción causada Exitosamente");
    }

    private static object BuildCtx(ConfigurationParameter? p)
    {
        return new { Exists = p is not null, Active = p?.Status ?? false };
    }
}