using Common.SharedKernel.Application.Messaging;
using Operations.Domain.Banks;

namespace Operations.Integrations.Banks;

public sealed record class GetBanksQuery : IQuery<IReadOnlyCollection<Bank>>;