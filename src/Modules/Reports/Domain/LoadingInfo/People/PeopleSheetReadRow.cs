namespace Reports.Domain.LoadingInfo.People;

/// <summary>
/// Fila de datos leída desde la fuente para el sheet de afiliados.
/// </summary>
public sealed record PeopleSheetReadRow
{
    public required long MemberId { get; init; }
    public required string IdentificationType { get; init; }
    public required string IdentificationTypeHomologated { get; init; }
    public required string Identification { get; init; }
    public required string FullName { get; init; }
    public DateTime? Birthday { get; init; }
    public required string Gender { get; init; }
}
