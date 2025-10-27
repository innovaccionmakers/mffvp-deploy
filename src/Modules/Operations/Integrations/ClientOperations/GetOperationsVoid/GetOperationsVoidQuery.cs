using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.ClientOperations.GetOperationsVoid;

public sealed record GetOperationsVoidQuery(
    int AffiliateId,
    int ObjectiveId,
    long OperationTypeId,
    int PageNumber,
    int PageSize) : IQuery<GetOperationsVoidResponse>;
