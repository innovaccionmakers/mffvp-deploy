using Common.SharedKernel.Domain;

namespace Products.Domain.Portfolios;
public sealed class PortfolioCreatedDomainEvent(long portfolioId) : DomainEvent
{
    public long PortfolioId { get; } = portfolioId;
}