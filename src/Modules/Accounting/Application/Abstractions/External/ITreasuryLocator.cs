using Common.SharedKernel.Domain;

namespace Accounting.Application.Abstractions.External;

public interface ITreasuryLocator
{
    Task<Result<IReadOnlyCollection<IssuerInfo>>> GetIssuersByIdsAsync(
        IEnumerable<long> ids,
        CancellationToken cancellationToken);
}

public sealed record IssuerInfo(
    long Id,
    string Nit,
    int Digit,
    string Description
);

