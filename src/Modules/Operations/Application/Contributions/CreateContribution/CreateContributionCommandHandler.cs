using Associate.IntegrationEvents.ActivateValidation;
using Common.SharedKernel.Application.Attributes;
using System.Text.Json;
using Operations.Domain.AuxiliaryInformations;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.Rules;
using Operations.Domain.Channels;
using Operations.Domain.ClientOperations;
using Operations.Domain.ConfigurationParameters;
using Operations.Domain.Origins;
using Operations.Domain.SubtransactionTypes;
using Operations.Integrations.Contributions;
using Operations.Integrations.Contributions.CreateContribution;
using People.IntegrationEvents.ClientValidation;
using Products.IntegrationEvents.ContributionValidation;
using Trusts.IntegrationEvents.CreateTrust;

namespace Operations.Application.Contributions.CreateContribution;

internal sealed class CreateContributionCommandHandler(
    IChannelRepository channelRepository,
    ISubtransactionTypeRepository subtypeRepo,
    IOriginRepository originRepository,
    IConfigurationParameterRepository configurationParameterRepository,
    IRuleEvaluator<OperationsModuleMarker> ruleEvaluator,
    ICapRpcClient rpc,
    IAuxiliaryInformationRepository auxiliaryInformationRepository,
    IClientOperationRepository clientOperationRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateContributionCommand, ContributionResponse>
{
    private const string ContributionValidationWorkflow = "Operations.Contribution.Validation";

    public async Task<Result<ContributionResponse>> Handle(CreateContributionCommand request,
        CancellationToken cancellationToken)
    {
        var rawCertified = request.CertifiedContribution?.Trim().ToUpperInvariant();
        var certifiedValid = string.IsNullOrEmpty(rawCertified)
                             || rawCertified is "SI" or "NO";

        var contributionSource = await originRepository
            .FindByHomologatedCodeAsync(request.Origin, cancellationToken);

        var scopes = new[]
        {
            (Code: request.OriginModality,
                Scope: HomologScope.Of<CreateContributionCommand>(c => c.OriginModality)),
            (Code: request.CollectionMethod,
                Scope: HomologScope.Of<CreateContributionCommand>(c => c.CollectionMethod)),
            (Code: request.PaymentMethod,
                Scope: HomologScope.Of<CreateContributionCommand>(c => c.PaymentMethod))
        };

        var batchCfg = await configurationParameterRepository
            .GetByCodesAndTypesAsync(scopes, cancellationToken);

        var originModality = batchCfg.GetValueOrDefault(scopes[0]);
        var collectionMethod = batchCfg.GetValueOrDefault(scopes[1]);
        var paymentMethod = batchCfg.GetValueOrDefault(scopes[2]);

        var activateValidation = await rpc.CallAsync<
            GetActivateIdByIdentificationRequest,
            GetActivateIdByIdentificationResponse>(
            nameof(GetActivateIdByIdentificationRequest),
            new GetActivateIdByIdentificationRequest(request.TypeId, request.Identification),
            TimeSpan.FromSeconds(5),
            cancellationToken);

        var activate = activateValidation.Activate!;

        if (!activateValidation.Succeeded)
            return Result.Failure<ContributionResponse>(
                Error.Validation(activateValidation.Code, activateValidation.Message));

        var contributionValidation = await rpc.CallAsync<
            ContributionValidationRequest,
            ContributionValidationResponse>(
            nameof(ContributionValidationRequest),
            new ContributionValidationRequest(activate.ActivateId, request.ObjectiveId,
                request.PortfolioId, request.DepositDate,
                request.ExecutionDate, request.Amount),
            TimeSpan.FromSeconds(5),
            cancellationToken);

        if (!contributionValidation.IsValid)
            return Result.Failure<ContributionResponse>(
                Error.Validation(
                    contributionValidation.Code,
                    contributionValidation.Message));

        var hasPrevious = await clientOperationRepository.ExistsContributionAsync(
            contributionValidation.AffiliateId!.Value,
            contributionValidation.ObjectiveId!.Value,
            contributionValidation.PortfolioId!.Value,
            cancellationToken);

        var isFirstContribution = !hasPrevious;

        var subtype = string.IsNullOrWhiteSpace(request.Subtype)
            ? await subtypeRepo.GetByNameAsync("Ninguno", cancellationToken)
            : await subtypeRepo.GetByHomologatedCodeAsync(request.Subtype!, cancellationToken);
        
        if (string.IsNullOrWhiteSpace(request.Subtype) && subtype is null)
            throw new InvalidOperationException(
                "El SubtransactionType por defecto 'Ninguno' no se encuentra configurado.");

        var subtypeExists = subtype is not null;

        var config = subtypeExists
            ? await configurationParameterRepository.GetByUuidAsync(subtype!.Category, cancellationToken)
            : null;

        var categoryIsContribution = config?.Name == "Aporte";

        var channel = await channelRepository.FindByHomologatedCodeAsync(
            request.Channel, cancellationToken);

        var channelExists = channel is not null;

        var validationContext = new
        {
            ContributionSource = new
            {
                Exists = contributionSource is not null,
                Active = contributionSource?.Status.Equals("A", StringComparison.OrdinalIgnoreCase) == true,
                RequiresCertification = contributionSource?.RequiresCertification ?? false
            },
            OriginModality = new
            {
                Exists = originModality is not null,
                Active = originModality?.Status ?? false
            },
            CollectionMethod = new
            {
                Exists = collectionMethod is not null,
                Active = collectionMethod?.Status ?? false
            },
            PaymentMethod = new
            {
                Exists = paymentMethod is not null,
                Active = paymentMethod?.Status ?? false
            },
            Channel = new
            {
                Exists = channelExists
            },
            IsFirstContribution = isFirstContribution,
            PortfolioInitialMinimumAmount = contributionValidation.PortfolioInitialMinimumAmount!.Value,
            Amount = request.Amount,
            CertifiedContributionProvided = !string.IsNullOrWhiteSpace(request.CertifiedContribution),
            CertifiedContributionValid = certifiedValid,
            SubtypeExists = subtypeExists,
            CategoryIsContribution = categoryIsContribution
        };

        var (ok, _, errors) = await ruleEvaluator.EvaluateAsync(
            ContributionValidationWorkflow,
            validationContext,
            cancellationToken);

        if (!ok)
        {
            var first = errors.First();
            return Result.Failure<ContributionResponse>(
                Error.Validation(first.Code, first.Message));
        }

        var personValidation = await rpc.CallAsync<
            ValidatePersonByIdentificationRequest,
            ValidatePersonByIdentificationResponse>(
            nameof(ValidatePersonByIdentificationRequest),
            new ValidatePersonByIdentificationRequest(request.TypeId, request.Identification),
            TimeSpan.FromSeconds(5),
            cancellationToken);

        if (!personValidation.IsValid)
            return Result.Failure<ContributionResponse>(
                Error.Validation(
                    personValidation.Code,
                    personValidation.Message));

        var isPensioner = activate.Pensioner;
        var isCertified = request.CertifiedContribution?.Trim().ToUpperInvariant() == "SI";

        var neededUuids = new List<Guid>
        {
            isPensioner
                ? ConfigurationParameterUuids.TaxExempt
                : isCertified
                    ? ConfigurationParameterUuids.TaxNoRetention
                    : ConfigurationParameterUuids.TaxRetention,

            isCertified
                ? ConfigurationParameterUuids.CertifiedState
                : ConfigurationParameterUuids.UncertifiedState
        };

        if (!isPensioner && !isCertified)
            neededUuids.Add(ConfigurationParameterUuids.RetentionPct);

        var configsByUuid = await configurationParameterRepository.GetByUuidsAsync(neededUuids, cancellationToken);
        var taxConditionId = configsByUuid[neededUuids[0]].ConfigurationParameterId;
        var certificationStatusId = configsByUuid[neededUuids[1]].ConfigurationParameterId;

        var retentionPct = configsByUuid.TryGetValue(ConfigurationParameterUuids.RetentionPct, out var pctParam)
            ? decimal.TryParse(pctParam.Metadata.RootElement.GetString()?.TrimEnd('%'), out var p)
                ? p / 100m
                : 0m
            : 0m;

        var withheldAmount = retentionPct > 0
            ? request.Amount * retentionPct
            : 0m;

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var operationResult = ClientOperation.Create(
            DateTime.UtcNow,
            contributionValidation.AffiliateId!.Value,
            contributionValidation.ObjectiveId!.Value,
            contributionValidation.PortfolioId!.Value,
            request.Amount,
            request.ExecutionDate,
            subtype?.SubtransactionTypeId ?? 0);

        clientOperationRepository.Insert(operationResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var auxResult = AuxiliaryInformation.Create(
            operationResult.Value.ClientOperationId,
            contributionSource!.OriginId,
            collectionMethod!.ConfigurationParameterId,
            paymentMethod!.ConfigurationParameterId,
            int.TryParse(request.CollectionAccount, out var account) ? account : 0,
            request.PaymentMethodDetail ?? JsonDocument.Parse("{}"),
            certificationStatusId,
            taxConditionId,
            request.ContingentWithholding.HasValue ? (int)request.ContingentWithholding.Value : 0,
            request.VerifiableMedium ?? JsonDocument.Parse("{}"),
            request.CollectionBank,
            request.DepositDate,
            request.SalesUser,
            originModality!.ConfigurationParameterId,
            0,
            channel?.ChannelId ?? 0,
            int.TryParse(request.User, out var userId) ? userId : 0
        );

        auxiliaryInformationRepository.Insert(auxResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var trustCreation = await rpc.CallAsync<
            CreateTrustRequest,
            CreateTrustResponse>(
            nameof(CreateTrustRequest),
            new CreateTrustRequest(
                contributionValidation.AffiliateId!.Value,
                operationResult.Value.ClientOperationId,
                DateTime.UtcNow,
                contributionValidation.ObjectiveId!.Value,
                contributionValidation.PortfolioId!.Value,
                request.Amount,
                0,
                request.Amount,
                0m,
                taxConditionId,
                withheldAmount,
                0m,
                request.Amount),
            TimeSpan.FromSeconds(5),
            cancellationToken);

        if (!trustCreation.Succeeded)
            return Result.Failure<ContributionResponse>(
                Error.Validation(trustCreation.Code ?? string.Empty, trustCreation.Message ?? string.Empty));


        var taxConditionParam = configsByUuid[neededUuids[0]];

        var response = new ContributionResponse(
            operationResult.Value.ClientOperationId,
            request.PortfolioId,
            contributionValidation.PortfolioName,
            taxConditionParam.Name,
            withheldAmount
        );

        await transaction.CommitAsync(cancellationToken);

        return Result.Success(response, "Transacción causada Exitosamente");
    }
}