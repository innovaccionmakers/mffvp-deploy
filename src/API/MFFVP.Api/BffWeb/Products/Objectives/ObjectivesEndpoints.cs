using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using MFFVP.Api.Application.Products;
using Products.Integrations.Objectives.CreateObjective;
using Products.Integrations.Objectives.GetObjectives;

namespace MFFVP.Api.BffWeb.Products.Objectives;

public sealed class ObjectivesEndpoints
{
    private readonly IObjectivesService _objectivesService;

    public ObjectivesEndpoints(IObjectivesService objectivesService)
    {
        _objectivesService = objectivesService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("bffWeb/FVP/Product")
            .WithTags("BFF Web - Objectives")
            .WithOpenApi();

        group.MapGet(
                "GetGoals/{typeId}/{identification}/{status}",
                async (
                    string typeId,
                    string identification,
                    StatusType status,
                    ISender sender
                ) =>
                {
                    var result = await _objectivesService
                        .GetObjectivesAsync(typeId, identification, status, sender);
                    return result.ToApiResult();
                }
            )
            .Produces<GetObjectivesResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        
        group.MapPost(
                "CreateGoal",
                async (
                    CreateObjectiveCommand comando,
                    ISender sender
                ) =>
                {
                    var resultado = await _objectivesService
                        .CreateObjectiveAsync(comando, sender);
                    return resultado.ToApiResult();
                }
            )
            .AddEndpointFilter<TechnicalValidationFilter<CreateObjectiveCommand>>()
            .Accepts<CreateObjectiveCommand>("application/json")
            .Produces<ObjectiveResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}