
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;
using Trusts.Integrations.DataSync.TrustSync.Response;

namespace Trusts.Integrations.DataSync.TrustSync.Queries;

public sealed record GetActiveTrustsByPortfolioQuery(int PortfolioId)
    : IQuery<IReadOnlyCollection<GetActiveTrustByPortfolioResponse>>;
