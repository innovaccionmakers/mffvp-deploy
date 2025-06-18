using Common.SharedKernel.Application.Messaging;
using Products.Domain.Banks;

namespace Products.Integrations.Banks;

public sealed record class GetBanksQuery : IQuery<IReadOnlyCollection<Bank>>;
    