using Associate.IntegrationEvents.ActivateValidation;
using Common.SharedKernel.Application.Attributes;
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

namespace Operations.Application.Contributions.CreateContribution;

internal sealed class CreateContributionCommandHandler(
    IChannelRepository channelRepository,
    ISubtransactionTypeRepository subtypeRepo,
    IOriginRepository originRepository,
    IConfigurationParameterRepository configurationParameterRepository,
    IRuleEvaluator<OperationsModuleMarker> ruleEvaluator,
    ICapRpcClient rpc,
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

        var originModality = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.OriginModality,
            HomologScope.Of<CreateContributionCommand>(c => c.OriginModality),
            cancellationToken);

        var collectionMethod = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.CollectionMethod,
            HomologScope.Of<CreateContributionCommand>(c => c.CollectionMethod),
            cancellationToken);

        var paymentMethod = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.PaymentMethod,
            HomologScope.Of<CreateContributionCommand>(c => c.PaymentMethod),
            cancellationToken);

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
            ? null
            : await subtypeRepo.GetByHomologatedCodeAsync(request.Subtype!, cancellationToken);

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
                Active = contributionSource?.Status.Equals("A", StringComparison.OrdinalIgnoreCase) == true
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
        
        var pensioner = activate.Pensioner;
        
        var exemptOption = await configurationParameterRepository.GetByCodeAndScopeAsync(
            "1125",
            "CondicionTributaria",
            cancellationToken);

        var noRetentionOption = await configurationParameterRepository.GetByCodeAndScopeAsync(
            "1126",
            "CondicionTributaria",
            cancellationToken);

        var retentionOption = await configurationParameterRepository.GetByCodeAndScopeAsync(
            "1128",
            "CondicionTributaria",
            cancellationToken);

        var retentionPctParam = await configurationParameterRepository.GetByCodeAndScopeAsync(
            "1129",
            "Porcentaje Retención Contingente",
            cancellationToken);

        decimal pct = 0.07m;
        var strPct = retentionPctParam?.Metadata.RootElement.GetString();
        if (!string.IsNullOrWhiteSpace(strPct) &&
            decimal.TryParse(strPct.TrimEnd('%'), out var parsed))
            pct = parsed / 100m;

        int certificationStatusId;
        decimal withheld = 0;

        if (pensioner)
        {
            certificationStatusId = exemptOption!.ConfigurationParameterId;
        }
        else if (rawCertified is not null && rawCertified == "SI")
        {
            certificationStatusId = noRetentionOption!.ConfigurationParameterId;
        }
        else
        {
            certificationStatusId = retentionOption!.ConfigurationParameterId;
            withheld = request.Amount * pct;
        }

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

        var response = new ContributionResponse(
            "Success",
            201,
            "Contribución procesada correctamente",
            123456789,
            request.PortfolioId,
            "Cartera Principal",
            "RESIDENT",
            0.0m
        );

        await transaction.CommitAsync(cancellationToken);

        return Result.Success(response);
    }
}