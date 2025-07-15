using Common.SharedKernel.Application.Messaging;
using Operations.Domain.SubtransactionTypes;

namespace Operations.Integrations.SubTransactionTypes;

public sealed record class GetAllOperationTypesQuery : IQuery<IReadOnlyCollection<SubtransactionTypeResponse>>;
