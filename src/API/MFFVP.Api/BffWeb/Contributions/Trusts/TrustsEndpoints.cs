using MediatR;
using Contributions.Integrations.Trusts.UpdateTrust;
using Contributions.Integrations.Trusts;
using Common.SharedKernel.Presentation.Results;
using Microsoft.AspNetCore.Mvc;
using MFFVP.Api.Application.Contributions;
using Contributions.Integrations.Trusts.CreateTrust;

namespace MFFVP.Api.BffWeb.Contributions.Trusts
{
    public sealed class TrustsEndpoints
    {
        private readonly ITrustsService _trustsService;

        public TrustsEndpoints(ITrustsService trustsService)
        {
            _trustsService = trustsService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("bffWeb/api/contributions/trusts")
                .WithTags("BFF Web - Trusts")
                .WithOpenApi();

            group.MapGet("GetAll", async (ISender sender) =>
            {
                var result = await _trustsService.GetTrustsAsync(sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .Produces<IReadOnlyCollection<TrustResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapGet("GetById/{id}", async (Guid id, ISender sender) =>
            {
                var result = await _trustsService.GetTrustAsync(id, sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .Produces<TrustResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPost("Create", async ([FromBody] CreateTrustCommand request, ISender sender) =>
            {
                var result = await _trustsService.CreateTrustAsync(request, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPut("Update", async ([FromBody] UpdateTrustCommand command, ISender sender) =>
            {
                var result = await _trustsService.UpdateTrustAsync(command, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapDelete("Delete/{id}", async (Guid id, ISender sender) =>
            {
                var result = await _trustsService.DeleteTrustAsync(id, sender);
                return result.Match(() => Results.NoContent(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}