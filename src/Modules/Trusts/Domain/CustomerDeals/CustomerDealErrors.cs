using Common.SharedKernel.Domain;

namespace Trusts.Domain.CustomerDeals;

public static class CustomerDealErrors
{
    public static Error NotFound(Guid customerdealId)
    {
        return Error.NotFound(
            "CustomerDeal.NotFound",
            $"The customerdeal with identifier {customerdealId} was not found"
        );
    }
}