using Common.SharedKernel.Domain;

namespace Contributions.Domain.Trusts;
public static class TrustErrors
{
    public static Error NotFound(Guid trustId) =>
        Error.NotFound(
            "Trust.NotFound",
            $"The trust with identifier {trustId} was not found"
        );
}