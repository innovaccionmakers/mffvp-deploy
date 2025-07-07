using Common.SharedKernel.Application.Messaging;
using Products.Domain.Offices;

namespace Products.Integrations.Offices;

public sealed record GetOfficesQuery : IQuery<IReadOnlyCollection<Office>>;
