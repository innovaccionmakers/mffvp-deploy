using Common.SharedKernel.Application.Messaging;
using Products.Domain.Commercials;

namespace Products.Integrations.Commercials;

public sealed record GetCommercialsQuery : IQuery<IReadOnlyCollection<Commercial>>;
