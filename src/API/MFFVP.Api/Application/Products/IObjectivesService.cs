using Common.SharedKernel.Domain;
using MediatR;

using Products.Integrations.Objectives;
using Products.Integrations.Objectives.GetObjectives;
using Products.Integrations.Objectives.UpdateObjective;

namespace MFFVP.Api.Application.Products
{
    public interface IObjectivesService
    {
        Task<Result<GetObjectivesResponse>> GetObjectivesAsync(
            string typeId,
            string identification,
            StatusType status,
            ISender sender
        );
    }
}