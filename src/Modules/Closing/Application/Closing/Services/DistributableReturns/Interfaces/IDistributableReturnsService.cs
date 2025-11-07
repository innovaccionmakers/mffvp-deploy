using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.DistributableReturns.Interfaces;

public interface IDistributableReturnsService
{
    Task<Result> RunAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}
