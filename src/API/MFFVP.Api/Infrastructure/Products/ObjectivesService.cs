using Common.SharedKernel.Domain;
using MediatR;
using Products.Integrations.Objectives.GetObjectives;
using MFFVP.Api.Application.Products;
using Products.Integrations.Objectives.CreateObjective;

namespace MFFVP.Api.Infrastructure.Products;

public sealed class ObjectivesService : IObjectivesService
{
    public Task<Result<IReadOnlyCollection<ObjectiveItem>>> GetObjectivesAsync(
        string typeId,
        string identification,
        StatusType status,
        ISender sender)
    {
        return sender.Send(new GetObjectivesQuery(typeId, identification, status));
    }
    
    private static StatusType MapStatus(string? raw) =>
        raw?.Trim().ToUpperInvariant() switch
        {
            "A" => StatusType.A,
            "I" => StatusType.I,
            "T" => StatusType.T,
            _   => StatusType.Unknown
        };
    
    public Task<Result<ObjectiveResponse>> CreateObjectiveAsync(
        CreateObjectiveCommand command,
        ISender sender)
    {
        return sender.Send(command);
    }
}