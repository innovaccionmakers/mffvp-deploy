using Closing.Integrations.PortfolioValuations.Response;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.PortfolioValuations.Queries;

public sealed record CheckValuationExistsQuery(
    long PortfolioId
) : IQuery<CheckValuationExistsResponse>;
