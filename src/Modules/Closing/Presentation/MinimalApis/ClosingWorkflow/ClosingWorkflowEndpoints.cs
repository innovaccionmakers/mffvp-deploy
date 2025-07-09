using Closing.Application.ClosingWorkflow;
using Closing.Domain.Routes;
using Common.SharedKernel.Presentation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Closing.Presentation.MinimalApis.ClosingWorkflow;

public static class ClosingWorkflowEndpoints
{
    public static void MapClosingWorkflowEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(Routes.RunClosing)
            .WithTags(TagName.TagClosing)
            .WithOpenApi();

        group.MapPost(
                "Begin",
                async ([FromBody] BeginClosingRequest request, IClosingWorkflowService service, CancellationToken ct) =>
                {
                    await service.BeginAsync(request.PortfolioId, request.ClosingDate, ct);
                    return Results.Ok(true);
                }
            )
            .WithName("BeginClosing")
            .WithSummary("Inicia el proceso de cierre para un portafolio")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost(
                "End",
                async ([FromBody] EndClosingRequest request, IClosingWorkflowService service, CancellationToken ct) =>
                {
                    await service.EndAsync(request.PortfolioId, ct);
                    return Results.Ok(true);
                }
            )
            .WithName("EndClosing")
            .WithSummary("Finaliza el proceso de cierre para un portafolio")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}

public sealed record BeginClosingRequest(int PortfolioId, DateTime ClosingDate);
public sealed record EndClosingRequest(int PortfolioId);