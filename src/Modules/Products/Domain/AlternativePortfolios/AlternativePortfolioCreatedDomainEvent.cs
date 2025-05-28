using Common.SharedKernel.Domain;

namespace Products.Domain.AlternativePortfolios;

public sealed class AlternativePortfolioCreatedDomainEvent(int alternativeportfolioId) : DomainEvent
{
    public int AlternativePortfolioId { get; } = alternativeportfolioId;
}