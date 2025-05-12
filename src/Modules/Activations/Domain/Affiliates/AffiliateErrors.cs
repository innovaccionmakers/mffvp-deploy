using Common.SharedKernel.Domain;

namespace Activations.Domain.Affiliates;

public static class AffiliateErrors
{
    public static Error NotFound(int affiliateId)
    {
        return Error.NotFound(
            "Affiliate.NotFound",
            $"The affiliate with identifier {affiliateId} was not found"
        );
    }
}