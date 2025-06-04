using Common.SharedKernel.Domain;
using MediatR;
using Products.Integrations.Objectives.GetObjectives;
using MFFVP.Api.Application.Products;

namespace MFFVP.Api.Infrastructure.Products;

public sealed class ObjectivesService : IObjectivesService
{
    public Task<Result<GetObjectivesResponse>> GetObjectivesAsync(
        string typeId,
        string identification,
        StatusType status,
        ISender sender)
    {
        return sender.Send(new GetObjectivesQuery(typeId, identification, status));
    }
}