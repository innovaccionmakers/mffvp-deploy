using Closing.Domain.Routes;
using Closing.Integrations.Closing.RunClosing;

using Common.SharedKernel.Domain.Auth.Permissions;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Closing.Presentation.MinimalApis.Closing;

public static class ClosingEndpointMappings
{
    //TODO: Considerar usar otra forma de mapear los endpoints
    public static void MapRunClosingEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost(
            NameEndpoints.RunClosing,
            [Authorize(Policy = MakersPermissionsClosing.PolicyExecuteClosure)]
            async ([FromBody] RunClosingCommand request, ISender sender) =>
            {
                var result = await sender.Send(request);
                return result.ToApiResult();
            })
            .WithName(NameEndpoints.RunClosing)
            .WithSummary(Summary.RunClosing)
            .WithDescription(Description.RunClosing)
            .WithOpenApi(operation =>
            {
                operation.RequestBody.Description = RequestBodyDescription.RunClosing;
                return operation;
            })
            .AddEndpointFilter<TechnicalValidationFilter<RunClosingCommand>>()
            .Produces<bool>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    public static void MapConfirmClosingEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost(
            NameEndpoints.ConfirmClosing,
            async ([FromBody] ConfirmClosingCommand request, ISender sender) =>
            {
                var result = await sender.Send(request);
                return result.ToApiResult();
            })
            .WithName(NameEndpoints.ConfirmClosing)
            .WithSummary(Summary.ConfirmClosing)
            .WithDescription(Description.ConfirmClosing)
            .WithOpenApi(operation =>
            {
                operation.RequestBody.Description = RequestBodyDescription.ConfirmClosing;
                return operation;
            })
            .AddEndpointFilter<TechnicalValidationFilter<ConfirmClosingCommand>>()
            .Produces<bool>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    public static void MapCancelClosingEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost(
            NameEndpoints.CancelClosing,
            async ([FromBody] CancelClosingCommand request, ISender sender) =>
            {
                var result = await sender.Send(request);
                return result.ToApiResult();
            })
            .WithName(NameEndpoints.CancelClosing)
            .WithSummary(Summary.CancelClosing)
            .WithDescription(Description.CancelClosing)
            .WithOpenApi(operation =>
            {
                operation.RequestBody.Description = RequestBodyDescription.CancelClosing;
                return operation;
            })
            .AddEndpointFilter<TechnicalValidationFilter<CancelClosingCommand>>()
            .Produces<bool>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
