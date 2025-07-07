namespace Products.Domain.Offices;

public interface IOfficeRepository
{
    Task<IReadOnlyDictionary<string, Office>>
        GetByHomologatedCodesAsync(
            IEnumerable<string> codes,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Office>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}