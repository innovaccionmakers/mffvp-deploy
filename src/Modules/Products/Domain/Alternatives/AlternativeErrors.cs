using Common.SharedKernel.Domain;

namespace Products.Domain.Alternatives;
public static class AlternativeErrors
{
    public static Error NotFound(long alternativeId) =>
        Error.NotFound(
            "Alternative.NotFound",
            $"The alternative with identifier {alternativeId} was not found"
        );
}