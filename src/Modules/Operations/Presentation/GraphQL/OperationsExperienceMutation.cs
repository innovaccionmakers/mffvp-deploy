using Common.SharedKernel.Domain;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using MediatR;
using Operations.Integrations.Contributions.CreateContribution;
using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL.Inputs;
using System.Text.Json;

namespace Operations.Presentation.GraphQL;

public class OperationsExperienceMutation(IMediator mediator) : IOperationsExperienceMutation
{

    public async Task<GraphqlMutationResult<ContributionMutationResult>> RegisterContributionAsync(
        CreateContributionInput input,
        IValidator<CreateContributionInput> validator,
        CancellationToken cancellationToken = default
    )
    {
        var result = new GraphqlMutationResult<ContributionMutationResult>();
        try
        {
            var validationResult = await RequestValidator.Validate(input, validator);

            if (validationResult is not null)
            {
                result.AddError(validationResult.Error);
                return result;
            }

            var command = CreateContributionCommand(input);
            var commandResult = await mediator.Send(command, cancellationToken);

            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }

            var detalle = new
            {
                condicion_tributaria = input.CertifiedContribution?.ToLower().Trim() == "no" ? "Sin Retención Contingente" : string.Empty
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

    private static CreateContributionCommand CreateContributionCommand(CreateContributionInput input)
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
            input.ExecutionDate,
            input.SalesUser,
            JsonDocument.Parse(JsonSerializer.Serialize(input.VerifiableMedium)),
            input.Subtype,
            input.Channel,
            input.User
        );
    }
}
