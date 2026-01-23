namespace Reports.Domain.LoadingInfo.People;

public interface IPeopleSheetReadRepository
{
    /// <summary>
    /// Lee los datos de personas que han sido modificados desde el último timestamp especificado.
    /// </summary>
    /// <param name="lastRowVersion">Timestamp (UNIX EPOCH ms) de la última ejecución exitosa. Si es null, trae todos los datos.</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    IAsyncEnumerable<PeopleSheetReadRow> ReadPeopleAsync(
        long? lastRowVersion,
        CancellationToken cancellationToken);
}
