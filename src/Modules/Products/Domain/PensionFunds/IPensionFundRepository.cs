namespace Products.Domain.PensionFunds
{
    public interface IPensionFundRepository
    {
        Task<IReadOnlyCollection<PensionFund>> GetAllPensionFundsAsync(CancellationToken cancellationToken = default);
    }
}
