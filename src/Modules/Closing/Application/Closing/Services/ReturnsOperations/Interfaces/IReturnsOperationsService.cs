using System;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.ReturnsOperations.Interfaces;

public interface IReturnsOperationsService
{
    Task<Result> RunAsync(int portfolioId, DateTime valuationDate, bool isInternalProcess, CancellationToken cancellationToken);
}
