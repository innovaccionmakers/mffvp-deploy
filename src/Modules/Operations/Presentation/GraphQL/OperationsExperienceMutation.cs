using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;

using FluentValidation;

using MediatR;

using Operations.Application.Abstractions.Services.ContributionService;
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
    private static class Constants
    {
        public static class CertifiedContribution
        {
            public const string No = "no";
            public const string Si = "si";
        }

        public static class TaxCondition
        {
            public const string SinRetencionContingente = "Sin Retención Contingente";
            public const string ConRetencionContingente = "Con Retención Contingente";
        }
    }

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
            input.PortfolioId ?? "1", // Manejo básico cuando PortfolioId es null
            cancellationToken);

            var command = CreateContributionCommand(input, contributionData);
            var commandResult = await mediator.Send(command, cancellationToken);

            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }

            var condicionTributaria = GetTaxCondition(input.CertifiedContribution);
            var detalle = new
            {
                condicion_tributaria = condicionTributaria
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
                //input.ExecutionDate ?? default, // Fix: Provide a default value for nullable DateTime
                ExecutionDate: contributionData.ExecuteDate,
                //input.SalesUser,
                SalesUser: contributionData.SalesUser,
                JsonDocument.Parse(JsonSerializer.Serialize(input.VerifiableMedium)),
                input.Subtype,
                //input.Channel,
                Channel: contributionData.Channel,
                input.User
            );
    }

    private static string GetTaxCondition(string? certifiedContribution)
    {
        if (string.IsNullOrWhiteSpace(certifiedContribution))
            return string.Empty;

        var normalizedValue = certifiedContribution.ToLower().Trim();

        return normalizedValue switch
        {
            Constants.CertifiedContribution.No => Constants.TaxCondition.ConRetencionContingente,
            Constants.CertifiedContribution.Si => Constants.TaxCondition.SinRetencionContingente,
            _ => string.Empty
        };
    }
}
