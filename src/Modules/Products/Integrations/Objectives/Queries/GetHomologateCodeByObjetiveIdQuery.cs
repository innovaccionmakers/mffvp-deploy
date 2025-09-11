using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Objectives.Queries;

public sealed record GetHomologateCodeByObjetiveIdQuery(int ObjetiveId) : IQuery<string>;
