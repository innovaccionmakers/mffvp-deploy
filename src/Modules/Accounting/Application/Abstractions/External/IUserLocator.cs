using Common.SharedKernel.Domain;

namespace Accounting.Application.Abstractions.External;

public interface IUserLocator
{
    Task<Result<string?>> GetEmailUserAsync(string userName, CancellationToken ct);
}
