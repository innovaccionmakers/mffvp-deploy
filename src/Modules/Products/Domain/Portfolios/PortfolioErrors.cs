using Common.SharedKernel.Domain;

namespace Products.Domain.Portfolios;

public static class PortfolioErrors
{
    public static Error NotFound(long portfolioId)
    {
        return Error.NotFound(
            "Portfolio.NotFound",
            $"The portfolio with identifier {portfolioId} was not found"
        );
    }
}