using Common.SharedKernel.Domain;

namespace Trusts.Domain.TrustHistories;

public static class TrustHistoryErrors
{
    public static Error NotFound(long trusthistoryId)
    {
        return Error.NotFound(
            "TrustHistory.NotFound",
            $"The trusthistory with identifier {trusthistoryId} was not found"
        );
    }
}