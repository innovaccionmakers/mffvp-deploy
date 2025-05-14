using MediatR;
using Trusts.Integrations.TrustHistories.UpdateTrustHistory;
using Trusts.Integrations.TrustHistories;
using Common.SharedKernel.Presentation.Results;
using Microsoft.AspNetCore.Mvc;
using MFFVP.Api.Application.Trusts;
using Trusts.Integrations.TrustHistories.CreateTrustHistory;

namespace MFFVP.Api.BffWeb.Trusts.TrustHistories
{
    public sealed class TrustHistoriesEndpoints
    {
        private readonly ITrustHistoriesService _trusthistoriesService;

        public TrustHistoriesEndpoints(ITrustHistoriesService trusthistoriesService)
        {
            _trusthistoriesService = trusthistoriesService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("bffWeb/api/trusts/trusthistories")
                .WithTags("BFF Web - TrustHistories")
                .WithOpenApi();

            group.MapGet("GetAll", async (ISender sender) =>
            {
                var result = await _trusthistoriesService.GetTrustHistoriesAsync(sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .Produces<IReadOnlyCollection<TrustHistoryResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapGet("GetById/{trustHistoryId}", async (long trustHistoryId, ISender sender) =>
            {
                var result = await _trusthistoriesService.GetTrustHistoryAsync(trustHistoryId, sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .Produces<TrustHistoryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPost("Create", async ([FromBody] CreateTrustHistoryCommand request, ISender sender) =>
            {
                var result = await _trusthistoriesService.CreateTrustHistoryAsync(request, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPut("Update", async ([FromBody] UpdateTrustHistoryCommand command, ISender sender) =>
            {
                var result = await _trusthistoriesService.UpdateTrustHistoryAsync(command, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapDelete("Delete/{trustHistoryId}", async (long trustHistoryId, ISender sender) =>
            {
                var result = await _trusthistoriesService.DeleteTrustHistoryAsync(trustHistoryId, sender);
                return result.Match(() => Results.NoContent(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}