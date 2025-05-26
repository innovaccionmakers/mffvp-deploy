using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.Rules;
using Operations.Domain.ConfigurationParameters;
using Operations.Integrations.Contributions;
using Operations.Integrations.Contributions.CreateContribution;
using People.IntegrationEvents.PersonValidation;
using Products.IntegrationEvents.ContributionValidation;

namespace Operations.Application.Contributions.CreateContribution;

internal sealed class CreateContributionCommandHandler(
    IConfigurationParameterRepository configurationParameterRepository,
    IRuleEvaluator<OperationsModuleMarker> ruleEvaluator,
    ICapRpcClient rpc,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateContributionCommand, ContributionResponse>
{
    private const string ContributionValidationWorkflow = "Operations.Contribution.Validation";

    public async Task<Result<ContributionResponse>> Handle(CreateContributionCommand request,
        CancellationToken cancellationToken)
    {
        var contributionSource =
            await configurationParameterRepository.GetByHomologationCodeAsync(request.Origin, cancellationToken);
        var originModality =
            await configurationParameterRepository.GetByHomologationCodeAsync(request.OriginModality,
                cancellationToken);
        var collectionMethod =
            await configurationParameterRepository.GetByHomologationCodeAsync(request.CollectionMethod,
                cancellationToken);
        var paymentMethod =
            await configurationParameterRepository.GetByHomologationCodeAsync(request.PaymentMethod, cancellationToken);


        var validationContext = new
        {
            ContributionSource = new
            {
                Code = request.Origin,
                Exists = contributionSource is not null,
                Active = contributionSource?.Status ?? false
            },
            OriginModality = new
            {
                Code = request.OriginModality,
                Exists = originModality is not null,
                Active = originModality?.Status ?? false
            },
            CollectionMethod = new
            {
                Code = request.CollectionMethod,
                Exists = collectionMethod is not null,
                Active = collectionMethod?.Status ?? false
            },
            PaymentMethod = new
            {
                Code = request.PaymentMethod,
                Exists = paymentMethod is not null,
                Active = paymentMethod?.Status ?? false
            }
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

        var contributionValidation = await rpc.CallAsync<
            ContributionValidationRequest,
            ContributionValidationResponse>(
            nameof(ContributionValidationRequest),
            new ContributionValidationRequest(request.ObjectiveId, request.PortfolioId, request.DepositDate,
                request.ExecutionDate, request.Amount),
            TimeSpan.FromSeconds(5),
            cancellationToken);

        if (!contributionValidation.IsValid)
            return Result.Failure<ContributionResponse>(
                Error.Validation(
                    contributionValidation.Code,
                    contributionValidation.Message));
        
        

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        var response = new ContributionResponse(
            Status:                    "Success",
            ResponseCode:              201,
            ResponseDescription:       "Contribución procesada correctamente",
            OperationId:               123456789,
            PortfolioId:               request.PortfolioId,
            PortfolioName:             "Cartera Principal",
            TaxCondition:              "RESIDENT",
            ContingentWithholdingValue: 0.0m
        );
        
        await transaction.CommitAsync(cancellationToken);
        
        return Result.Success(response);
    }
}