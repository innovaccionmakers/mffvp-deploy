using Contributions.Domain.Portfolios;

namespace Contributions.Infrastructure.Mocks
{
    internal sealed class InMemoryPortfolioRepository : IPortfolioRepository
    {
        private static readonly List<Portfolio> _data =
        [
            new Portfolio(Code: "P01", Name: "Portafolio Liquidez", IsCollector: true, ObjectiveId: 1458, OperationDate: DateTime.UtcNow.Date),
    ];

        public Portfolio? GetByCode(string code)
            => _data.Find(p => p.Code == code);

        public bool BelongsToObjective(string code, int objectiveId)
            => _data.Exists(p => p.Code == code && p.ObjectiveId == objectiveId);
    }
}
