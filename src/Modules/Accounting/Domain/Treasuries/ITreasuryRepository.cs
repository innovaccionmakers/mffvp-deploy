namespace Accounting.Domain.Treasuries
{
    public interface ITreasuryRepository
    {
        Task<IEnumerable<Treasury>> GetAccountingConceptsTreasuriesAsync(IEnumerable<int> PortfolioIds, IEnumerable<string> AccountNumbers, CancellationToken CancellationToken);
        Task<IEnumerable<Treasury>> GetAccountingOperationsTreasuriesAsync(IEnumerable<int> PortfolioIds, IEnumerable<string> CollectionAccount, CancellationToken CancellationToken);
        Task<Treasury> GetTreasuryAsync(int PortfolioId, string BankAccount, CancellationToken CancellationToken);
        void Insert(Treasury Treasury);
        void Update(Treasury Treasury);
        void Delete(Treasury Treasury);
    }
}
