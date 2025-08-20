using Common.SharedKernel.Core.Primitives;

namespace Products.Domain.Commercials;

public static class CommercialErrors
{
    public static Error NotFound(int commercialId)
    {
        return Error.NotFound(
            "Commercial.NotFound",
            $"The commercial with identifier {commercialId} was not found"
        );
    }
}