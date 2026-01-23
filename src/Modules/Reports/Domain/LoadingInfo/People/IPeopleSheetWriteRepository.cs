namespace Reports.Domain.LoadingInfo.People;

public interface IPeopleSheetWriteRepository
{
    Task TruncateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Elimina selectivamente los registros de afiliados específicos antes de la carga incremental.
    /// </summary>
    /// <param name="memberIds">IDs de afiliados a eliminar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task DeleteByMemberIdsAsync(
        IReadOnlyCollection<long> memberIds,
        CancellationToken cancellationToken);

    Task BulkInsertAsync(
        IReadOnlyList<PeopleSheet> rows,
        CancellationToken cancellationToken);
}
