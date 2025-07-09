using Treasury.Domain.Issuers;

namespace Treasury.Domain.Issuers;

public interface IIssuerRepository
{
    Task<Issuer?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Issuer>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Issuer issuer);
}