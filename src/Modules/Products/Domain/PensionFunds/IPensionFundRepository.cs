namespace Products.Domain.PensionFunds
{
    public interface IPensionFundRepository
    {
        Task<IReadOnlyCollection<PensionFund>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
