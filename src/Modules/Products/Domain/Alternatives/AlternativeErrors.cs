using Common.SharedKernel.Domain;

namespace Products.Domain.Alternatives;

public static class AlternativeErrors
{
    public static Error NotFound(int alternativeId)
    {
        return Error.NotFound(
            "Alternative.NotFound",
            $"The alternative with identifier {alternativeId} was not found"
        );
    }
}