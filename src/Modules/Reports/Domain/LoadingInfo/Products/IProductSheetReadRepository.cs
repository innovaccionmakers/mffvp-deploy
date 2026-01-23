namespace Reports.Domain.LoadingInfo.Products;

public interface IProductSheetReadRepository
{
    /// <summary>
    /// Lee los datos de productos que han sido modificados desde el último timestamp especificado.
    /// </summary>
    /// <param name="lastRowVersion">Timestamp (UNIX EPOCH ms) de la última ejecución exitosa. Si es null, trae todos los datos.</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    IAsyncEnumerable<ProductSheetReadRow> ReadProductsAsync(
        long? lastRowVersion,
        CancellationToken cancellationToken);
}
