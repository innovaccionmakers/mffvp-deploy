using Common.SharedKernel.Presentation.Filters;

using FluentValidation;

using HotChocolate;
using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Operations.Integrations.Contributions.CreateContribution;
using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL.Inputs;
using System.Text.Json;

namespace Operations.Presentation.GraphQL;

public class OperationsExperienceMutation(IMediator mediator) : IOperationsExperienceMutation
{

    public async Task<ContributionMutationResult> RegisterContributionAsync(
        CreateContributionInput input,
        IValidator<CreateContributionInput> validator,
        CancellationToken cancellationToken = default
    )
    {
        try
        {

            var validationResult = await RequestValidator.Validate<CreateContributionInput>(input, validator);

            if (validationResult is not null)
            {
                var errorDetails = validationResult.Error != null
                    ? $"Código: {validationResult.Error.Code}, Descripción: {validationResult.Error.Description}, Tipo: {validationResult.Error.Type}"
                    : "Error desconocido";
                return new ContributionMutationResult
                (
                    false,
                    $"Error al registrar aporte: {errorDetails}",
                    null
                );
            }

            var command = new CreateContributionCommand(
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
                input.ExecutionDate ?? default, // Fix: Provide a default value for nullable DateTime
                input.SalesUser,
                JsonDocument.Parse(JsonSerializer.Serialize(input.VerifiableMedium)),
                input.Subtype,
                input.Channel,
                input.User
            );
            var result = await mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                var errorDetails = result.Error != null
                    ? $"Código: {result.Error.Code}, Descripción: {result.Error.Description}, Tipo: {result.Error.Type}"
                    : "Error desconocido";

                return new ContributionMutationResult(
                    false,
                    $"Error al registrar aporte: {errorDetails}",
                    null
                );
            }

            return new ContributionMutationResult
            (
                true,
                "Aporte registrado exitosamente.",
                result.Value
            );
        }
        catch (Exception ex)
        {
            return new ContributionMutationResult
            (
                false,
                $"Error al registrar aporte: {ex.Message}",
                null
            );
        }
    }

}
