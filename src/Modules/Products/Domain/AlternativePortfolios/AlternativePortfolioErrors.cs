using Common.SharedKernel.Core.Primitives;

namespace Products.Domain.AlternativePortfolios;

public static class AlternativePortfolioErrors
{
    public static Error NotFound(int alternativeportfolioId)
    {
        return Error.NotFound(
            "AlternativePortfolio.NotFound",
            $"The alternativeportfolio with identifier {alternativeportfolioId} was not found"
        );
    }
}