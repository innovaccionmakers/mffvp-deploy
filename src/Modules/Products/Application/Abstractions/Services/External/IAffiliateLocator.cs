using Common.SharedKernel.Domain;

namespace Products.Application.Abstractions.Services.External;

public interface IAffiliateLocator
{
    Task<Result<int?>> FindAsync(
        string docTypeCode,
        string identification,
        CancellationToken ct);
}