using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;

using FluentValidation;

using MediatR;

using Operations.Application.Abstractions.Services.ContributionService;
using Operations.Integrations.AccountingRecords.CreateDebitNote;
using Operations.Integrations.Contributions.CreateContribution;
using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL.Inputs;

using System.Text.Json;

namespace Operations.Presentation.GraphQL;

public class OperationsExperienceMutation(
    IMediator mediator,
    IBuildMissingFieldsContributionService buildMissingFieldsContributionService
) : IOperationsExperienceMutation
{    
    public async Task<GraphqlResult<ContributionMutationResult>> RegisterContributionAsync(
        CreateContributionInput input,
        IValidator<CreateContributionInput> validator,
        CancellationToken cancellationToken = default
    )
    {
        var result = new GraphqlResult<ContributionMutationResult>();
        try
        {
            var validationResult = await RequestValidator.Validate(input, validator);

            if (validationResult is not null)
            {
                result.AddError(validationResult.Error);
                return result;
            }

           
            var contributionData = await buildMissingFieldsContributionService.BuildAsync(
            input.PortfolioId,
            input.ObjectiveId,
            cancellationToken);

            var command = CreateContributionCommand(input, contributionData);
            var commandResult = await mediator.Send(command, cancellationToken);

            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }
            
            var detalle = new
            {
                condicion_tributaria = commandResult.Value.TaxCondition
            };

            var detalleJson = JsonDocument.Parse(JsonSerializer.Serialize(detalle));

            var response = new ContributionMutationResult(
                commandResult.Value.OperationId ?? 0,                
                "Comprobante",
                detalleJson.RootElement
            );

            result.SetSuccess(response, "Genial!, Se ha procesado la transacción de Aporte");
            return result;
        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }
    }

    public async Task<GraphqlResult<DebitNoteMutationResult>> RegisterDebitNoteAsync(
        CreateDebitNoteInput input,
        IValidator<CreateDebitNoteInput> validator,
        CancellationToken cancellationToken = default)
    {
        var result = new GraphqlResult<DebitNoteMutationResult>();

        try
        {
            var validationResult = await RequestValidator.Validate(input, validator);

            if (validationResult is not null)
            {
                result.AddError(validationResult.Error);
                return result;
            }

            var command = new AccountingRecordsValCommand(
                input.ClientOperationId,
                input.Amount,
                input.CauseId,
                input.AffiliateId,
                input.ObjectiveId);

            var commandResult = await mediator.Send(command, cancellationToken);

            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }

            var response = new DebitNoteMutationResult(
                commandResult.Value.DebitNoteId,
                commandResult.Value.Message);

            result.SetSuccess(response, commandResult.Value.Message);
            return result;
        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }
    }

    private static CreateContributionCommand CreateContributionCommand(
        CreateContributionInput input,
        (DateTime ExecuteDate, string Channel, string SalesUser) contributionData
    )
    {
        return new CreateContributionCommand(
                input.TypeId,
                input.Identification,
                input.ObjectiveId,
                input.PortfolioId,
                input.Amount,
                input.Origin,
                input.OriginModality,
                input.CollectionMethod,
                input.PaymentMethod,
                JsonDocument.Parse(JsonSerializer.Serialize(input.PaymentMethodDetail)),
                input.CollectionBank,
                input.CollectionAccount,
                input.CertifiedContribution,
                input.ContingentWithholding,
                input.DepositDate,
                ExecutionDate: contributionData.ExecuteDate,
                SalesUser: contributionData.SalesUser,
                JsonDocument.Parse(JsonSerializer.Serialize(input.VerifiableMedium)),
                input.Subtype,
                Channel: contributionData.Channel,
                input.User
            );
    }   
}
