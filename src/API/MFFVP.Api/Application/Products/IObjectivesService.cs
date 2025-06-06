using Common.SharedKernel.Domain;
using MediatR;

using Products.Integrations.Objectives;
using Products.Integrations.Objectives.CreateObjective;
using Products.Integrations.Objectives.GetObjectives;

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
        
        Task<Result<ObjectiveResponse>> CreateObjectiveAsync(
            CreateObjectiveCommand command,
            ISender sender
        );
    }
}