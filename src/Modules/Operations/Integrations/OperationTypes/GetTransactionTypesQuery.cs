using Common.SharedKernel.Application.Messaging;
using Operations.Domain.OperationTypes;

namespace Operations.Integrations.OperationTypes;

public sealed record class GetTransactionTypesQuery : IQuery<IReadOnlyCollection<OperationType>>;