using Treasury.Domain.Issuers;

namespace Treasury.Domain.Issuers;

public interface IIssuerRepository
{
    Task<Issuer?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Issuer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Issuer>> GetBanksAsync(CancellationToken cancellationToken = default);
    Task<Issuer?> GetByHomologatedCodeAsync(string homologatedCode, CancellationToken cancellationToken = default);
    void Add(Issuer issuer);
}