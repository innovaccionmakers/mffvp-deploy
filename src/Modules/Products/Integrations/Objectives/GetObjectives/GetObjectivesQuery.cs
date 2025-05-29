using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Objectives.GetObjectives;

public record GetObjectivesQuery(
    string TypeId,
    string Identification,
    StatusType Status
) : IQuery<GetObjectivesResponse>;