namespace Contributions.Domain.Portfolios
{
    public interface IPortfolioRepository
    {
        Portfolio? GetByCode(string code);
        bool BelongsToObjective(string code, int objectiveId);
    }
}
