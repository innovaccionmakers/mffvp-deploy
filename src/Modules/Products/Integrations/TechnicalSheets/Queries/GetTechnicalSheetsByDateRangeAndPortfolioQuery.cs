using Common.SharedKernel.Application.Messaging;
using Products.Integrations.TechnicalSheets.Response;

namespace Products.Integrations.TechnicalSheets.Queries;

public sealed record GetTechnicalSheetsByDateRangeAndPortfolioQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    int PortfolioId
) : IQuery<IReadOnlyCollection<TechnicalSheetResponse>>;