namespace Reports.Infrastructure.Options;

public sealed class ReportsOptions
{
    public string? ProductsSchema { get; init; }
    public string? ClosingSchema { get; init; }
    public string? KeywordTransmissionFormat { get; init; }
}
