
using Common.SharedKernel.Domain;
using MediatR;

using Products.Integrations.Objectives;
using Products.Integrations.Objectives.CreateObjective;
using Products.Integrations.Objectives.DeleteObjective;
using Products.Integrations.Objectives.GetObjective;
using Products.Integrations.Objectives.GetObjectives;
using Products.Integrations.Objectives.UpdateObjective;

using MFFVP.Api.Application.Products;

namespace MFFVP.Api.Infrastructure.Products
{
    public sealed class ObjectivesService : IObjectivesService
    {
        public async Task<Result<IReadOnlyCollection<ObjectiveResponse>>> GetObjectivesAsync(ISender sender)
        {
            return await sender.Send(new GetObjectivesQuery());
        }

        public async Task<Result<ObjectiveResponse>> GetObjectiveAsync(long objectiveId, ISender sender)
        {
            return await sender.Send(new GetObjectiveQuery(objectiveId));
        }

        public async Task<Result> CreateObjectiveAsync(CreateObjectiveCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> UpdateObjectiveAsync(UpdateObjectiveCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> DeleteObjectiveAsync(long objectiveId, ISender sender)
        {
            return await sender.Send(new DeleteObjectiveCommand(objectiveId));
        }
    }
}