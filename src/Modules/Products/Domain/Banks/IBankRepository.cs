namespace Products.Domain.Banks;

public interface IBankRepository
{
    Task<IReadOnlyCollection<Bank>> GetBanksAsync(
        CancellationToken cancellationToken = default);
}
