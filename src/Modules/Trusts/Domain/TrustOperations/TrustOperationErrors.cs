using Common.SharedKernel.Domain;

namespace Trusts.Domain.TrustOperations;

public static class TrustOperationErrors
{
    public static Error NotFound(Guid trustoperationId)
    {
        return Error.NotFound(
            "TrustOperation.NotFound",
            $"The trustoperation with identifier {trustoperationId} was not found"
        );
    }
}