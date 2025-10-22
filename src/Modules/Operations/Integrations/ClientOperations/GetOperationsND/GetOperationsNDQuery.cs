using System;
using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.ClientOperations.GetOperationsND;

public sealed record GetOperationsNDQuery(
    DateTime StartDate,
    DateTime EndDate,
    int AffiliateId,
    int ObjectiveId,
    int PageNumber,
    int PageSize) : IQuery<GetOperationsNDResponse>;
