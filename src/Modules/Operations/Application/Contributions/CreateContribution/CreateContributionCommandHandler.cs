using Associate.IntegrationEvents.ActivateValidation;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.Rules;
using Operations.Domain.ClientOperations;
using Operations.Domain.ConfigurationParameters;
using Operations.Integrations.Contributions;
using Operations.Integrations.Contributions.CreateContribution;
using People.IntegrationEvents.ClientValidation;
using Products.IntegrationEvents.ContributionValidation;

namespace Operations.Application.Contributions.CreateContribution;

internal sealed class CreateContributionCommandHandler(
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
        var contributionSource = await configurationParameterRepository.GetByCodeAndScopeAsync(
            request.Origin,
            HomologScope.Of<CreateContributionCommand>(c => c.Origin),
            cancellationToken);

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

        if (!activateValidation.Succeeded)
            return Result.Failure<ContributionResponse>(
                Error.Validation(activateValidation.Code, activateValidation.Message));

        var contributionValidation = await rpc.CallAsync<
            ContributionValidationRequest,
            ContributionValidationResponse>(
            nameof(ContributionValidationRequest),
            new ContributionValidationRequest(activateValidation.ActivateId!.Value, request.ObjectiveId,
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

        var validationContext = new
        {
            ContributionSource = new
            {
                Exists = contributionSource is not null,
                Active = contributionSource?.Status ?? false
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
            IsFirstContribution = isFirstContribution,
            PortfolioInitialMinimumAmount = contributionValidation.PortfolioInitialMinimumAmount!.Value,
            Amount = request.Amount
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

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

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