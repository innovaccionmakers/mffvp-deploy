using Common.SharedKernel.Domain;
namespace Products.Application.Abstractions.External.Trusts;

public interface ITrustYieldLocator
{
    Task<Result<int>> GetParticipant(IEnumerable<long> trustIds, CancellationToken cancellationToken = default);
}
