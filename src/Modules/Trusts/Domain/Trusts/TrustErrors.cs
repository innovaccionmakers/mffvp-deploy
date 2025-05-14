using Common.SharedKernel.Domain;

namespace Trusts.Domain.Trusts;

public static class TrustErrors
{
    public static Error NotFound(long trustId)
    {
        return Error.NotFound(
            "Trust.NotFound",
            $"The trust with identifier {trustId} was not found"
        );
    }
}