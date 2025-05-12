using Trusts.Domain.Portfolios;

namespace Trusts.Infrastructure.Mocks;

internal sealed class InMemoryPortfolioRepository : IPortfolioRepository
{
    private static readonly List<Portfolio> _data =
    [
        new(
            "P01",
            "Portafolio Liquidez",
            true,
            1458,
            DateTime.UtcNow.Date
        ),

        new(
            "P02",
            "Portafolio Inversión",
            false,
            2000,
            DateTime.UtcNow.Date
        ),

        new(
            "P03",
            "Portafolio Diferido",
            true,
            1458,
            DateTime.UtcNow.Date.AddDays(-1)
        )
    ];

    public Portfolio? GetByCode(string code)
    {
        return _data.Find(p => p.Code == code);
    }

    public bool BelongsToObjective(string code, int objectiveId)
    {
        return _data.Exists(p => p.Code == code && p.ObjectiveId == objectiveId);
    }
}