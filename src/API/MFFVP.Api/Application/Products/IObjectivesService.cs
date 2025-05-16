using Common.SharedKernel.Domain;
using MediatR;

using Products.Integrations.Objectives;
using Products.Integrations.Objectives.CreateObjective;
using Products.Integrations.Objectives.UpdateObjective;

namespace MFFVP.Api.Application.Products
{
    public interface IObjectivesService
    {
        Task<Result<IReadOnlyCollection<ObjectiveResponse>>> GetObjectivesAsync(ISender sender);
        Task<Result<ObjectiveResponse>> GetObjectiveAsync(long objectiveId, ISender sender);
        Task<Result> CreateObjectiveAsync(CreateObjectiveCommand request, ISender sender);
        Task<Result> UpdateObjectiveAsync(UpdateObjectiveCommand request, ISender sender);
        Task<Result> DeleteObjectiveAsync(long objectiveId, ISender sender);
    }
}