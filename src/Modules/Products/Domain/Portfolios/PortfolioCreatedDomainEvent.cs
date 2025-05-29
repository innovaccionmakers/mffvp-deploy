using Common.SharedKernel.Domain;

namespace Products.Domain.Portfolios;

public sealed class PortfolioCreatedDomainEvent(int portfolioId) : DomainEvent
{
    public int PortfolioId { get; } = portfolioId;
}