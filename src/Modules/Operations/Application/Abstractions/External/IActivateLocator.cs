using Common.SharedKernel.Domain;

namespace Operations.Application.Abstractions.External;

public interface IActivateLocator
{
    Task<Result<(bool Found, int ActivateId, bool IsPensioner)>>
        FindAsync(string idType, string identification, CancellationToken ct);
}