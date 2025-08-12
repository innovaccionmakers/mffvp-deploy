using Common.SharedKernel.Domain;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Trusts.Integrations.Trusts.CreateTrust;
using Trusts.Integrations.Trusts.GetBalances;

using Trusts.Integrations.Trusts;
using Trusts.Domain.Routes;

namespace Trusts.Presentation.MinimalApis;

public static class TrustsBusinessApi
{
    public static void MapTrustsBusinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(Routes.Trust)
            .WithTags(TagName.TagTrust)
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost(
                NameEndpoints.CreateTrust,
                async (
                    CreateTrustCommand command,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(command);
                    return result.ToApiResult();
                }
            )
            .WithName(NameEndpoints.CreateTrust)
            .WithSummary(Summary.CreateTrust)
            .WithDescription(Description.CreateTrust)
            .WithOpenApi(operation =>
            {
                operation.RequestBody.Description = RequestBodyDescription.CreateTrust;
                return operation;
            })
            .AddEndpointFilter<TechnicalValidationFilter<CreateTrustCommand>>()
            .Accepts<CreateTrustCommand>("application/json")
            .Produces<TrustResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet(
                NameEndpoints.GetBalances,
                async ([FromQuery] int affiliateId, ISender sender) =>
                {
                    var result = await sender.Send(new GetBalancesQuery(affiliateId));
                    return result.ToApiResult();
                }
            )
            .WithName(NameEndpoints.GetBalances)
            .WithSummary(Summary.GetBalances)
            .WithDescription(Description.GetBalances)
            .WithOpenApi(operation =>
            {
                var p = operation.Parameters.First(p => p.Name == "affiliateId");
                p.Description = "Identificador del afiliado";
                p.Example = new OpenApiInteger(1);
                return operation;
            })
            .Produces<IReadOnlyCollection<BalanceResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
