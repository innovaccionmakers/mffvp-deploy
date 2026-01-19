using System.Threading;
using System.Threading.Tasks;

namespace Products.Domain.Administrators;

public interface IAdministratorRepository
{
    Task<bool> ExistsByEntityCodeAsync(string entityCode, CancellationToken cancellationToken = default);
    Task<Administrator?> GetFirstOrderedByIdAsync(CancellationToken cancellationToken = default);
}

