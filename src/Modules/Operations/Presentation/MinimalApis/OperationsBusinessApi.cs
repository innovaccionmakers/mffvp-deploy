using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain.Auth.Permissions;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Operations.Domain.Routes;
using Operations.Domain.Routes;
using Operations.Integrations.Contributions.CreateContribution;
using Operations.Integrations.OperationTypes;

namespace Operations.Presentation.MinimalApis;

public static class OperationsBusinessApi
{
    public static void MapOperationsBusinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(Routes.Operation)
            .WithTags(TagName.TagOperation)
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost(
                NameEndpoints.CreateContribution,
                [Authorize(Policy = MakersPermissionsOperations.PolicyExecuteIndividualOperations)]
                async (
                    [Microsoft.AspNetCore.Mvc.FromBody] CreateContributionCommand request,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(request);
                    return result.ToApiResult();
                }
            )
            .WithName(NameEndpoints.CreateContribution)
            .WithSummary(Summary.CreateContribution)
            .WithDescription(Description.CreateContribution)
            .WithOpenApi(operation =>
            {
                operation.RequestBody.Description = RequestBodyDescription.CreateContribution;
                return operation;
            })
            .AddEndpointFilter<TechnicalValidationFilter<CreateContributionCommand>>()
            .Accepts<CreateContributionCommand>("application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        
        group.MapGet(
                NameEndpoints.GetAllOperationTypes,
                async (ISender sender) =>
                {
                    var result = await sender.Send(new GetAllOperationTypesQuery());
                    return result.Match(
                        Results.Ok,
                        r => r.Error.Type == ErrorType.Validation
                            ? ApiResults.Failure(r)
                            : ApiResults.Problem(r)
                    );
                }
            )
            .WithName(NameEndpoints.GetAllOperationTypes)
            .WithSummary(Summary.GetAllOperationTypes)
            .WithDescription(Description.GetAllOperationTypes)
            .Produces<IReadOnlyCollection<OperationTypeResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
