using Common.SharedKernel.Application.Messaging;
using Operations.Domain.SubtransactionTypes;

namespace Operations.Integrations.SubTransactionTypes;

public record class GetSubTransactionTypesQuery(
    Guid categoryId
) : IQuery<IReadOnlyCollection<SubtransactionType>>;
