namespace Products.Domain.Commercials;

public interface ICommercialRepository
{
    Task<Commercial?>
        GetByHomologatedCodeAsync(
            string code,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Commercial>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}