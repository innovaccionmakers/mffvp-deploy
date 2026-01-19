using Common.SharedKernel.Domain;
using Products.Integrations.Administrators;

namespace Accounting.Application.Abstractions.External;

public interface IAdministratorLocator
{
    Task<Result<AdministratorResponse?>> GetFirstAdministratorAsync(CancellationToken cancellationToken = default);
}

