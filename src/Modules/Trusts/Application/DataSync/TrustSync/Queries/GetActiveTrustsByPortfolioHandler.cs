
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;
using Trusts.Domain.Trusts;
using Trusts.Integrations.DataSync.TrustSync.Queries;
using Trusts.Integrations.DataSync.TrustSync.Response;

namespace Trusts.Application.DataSync.TrustSync.Queries;

public sealed class GetActiveTrustsByPortfolioHandler
    : IQueryHandler<GetActiveTrustsByPortfolioQuery, IReadOnlyCollection<GetActiveTrustByPortfolioResponse>>
{
    private readonly ITrustRepository _repository;

    public GetActiveTrustsByPortfolioHandler(ITrustRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyCollection<GetActiveTrustByPortfolioResponse>>> Handle(GetActiveTrustsByPortfolioQuery query, CancellationToken ct)
    {
        var trusts = await _repository.GetActiveTrustsByPortfolioAsync(query.PortfolioId, ct);

        var activeTrusts = trusts
            .Select(t => new GetActiveTrustByPortfolioResponse
            {
                TrustId = t.TrustId,
                PortfolioId = t.PortfolioId,
                TotalBalance = t.TotalBalance,
                Principal = t.Principal,
                ContingentWithholding = t.ContingentWithholding
            });

        return activeTrusts.ToList();
    }
}
