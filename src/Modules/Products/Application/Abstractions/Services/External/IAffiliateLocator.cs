namespace Products.Application.Abstractions.Services.External;

public interface IAffiliateLocator
{
    Task<(bool Found, int? Id)> FindAsync(
        string docTypeCode,
        string identification,
        CancellationToken ct);
}