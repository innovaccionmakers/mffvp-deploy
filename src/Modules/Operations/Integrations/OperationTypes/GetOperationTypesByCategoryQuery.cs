using Common.SharedKernel.Application.Messaging;
using Operations.Domain.OperationTypes;

namespace Operations.Integrations.OperationTypes;

public record class GetOperationTypesByCategoryQuery(
    int? categoryId
) : IQuery<IReadOnlyCollection<OperationType>>;
