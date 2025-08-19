using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Domain;

namespace Operations.Application.Abstractions.External;

public interface ICollectionBankValidator
{
    Task<Result<long>> ValidateAsync(string homologatedCode, CancellationToken cancellationToken = default);
}