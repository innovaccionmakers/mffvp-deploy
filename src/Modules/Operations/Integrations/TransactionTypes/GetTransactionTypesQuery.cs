using Common.SharedKernel.Application.Messaging;
using Operations.Domain.TransactionTypes;

namespace Operations.Integrations.TransactionTypes;

public sealed record class GetTransactionTypesQuery : IQuery<IReadOnlyCollection<TransactionType>>;
