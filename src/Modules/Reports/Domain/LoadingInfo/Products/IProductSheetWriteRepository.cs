namespace Reports.Domain.LoadingInfo.Products;

public interface IProductSheetWriteRepository
{
    Task TruncateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Elimina selectivamente los registros de productos específicos antes de la carga incremental.
    /// </summary>
    /// <param name="fundIds">IDs de fondos a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task DeleteByFundIdsAsync(
        IReadOnlyCollection<int> fundIds,
        CancellationToken cancellationToken);

    Task BulkInsertAsync(IReadOnlyList<ProductSheet> rows, CancellationToken cancellationToken);
}
