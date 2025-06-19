using Common.SharedKernel.Domain.ConfigurationParameters;
namespace Operations.Domain.ConfigurationParameters;

public interface IConfigurationParameterRepository : IConfigurationParameterLookupRepository
{
    Task<IReadOnlyDictionary<(string Code, string Scope), ConfigurationParameter>>
        GetByCodesAndTypesAsync(
            IEnumerable<(string Code, string Scope)> pairs,
            CancellationToken cancellationToken = default);


    Task<ConfigurationParameter?> GetByCodeAndScopeAsync(
        string homologationCode,
        string scope,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, ConfigurationParameter>>
        GetByUuidsAsync(IEnumerable<Guid> uuids, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TransactionType>> GetTransactionTypesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DocumentType>> GetDocumentTypesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CertificationStatus>> GetCertificationStatusesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OriginMode>> GetOriginModesAsync(
        CancellationToken cancellationToken = default);
}