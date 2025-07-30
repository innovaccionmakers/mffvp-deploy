
using MediatR;

namespace Products.Integrations.PortfolioValuation.Commands;

public sealed record UpsertPortfolioValuationCommand(
    int PortfolioId,
    DateTime CloseDate,
    decimal Value,
    decimal Units,
    decimal UnitValue,
    decimal GrossYieldUnits,
    decimal UnitCost,
    decimal DailyYield,
    decimal IncomingOperations,
    decimal OutgoingOperations,
    DateTime ProcessDate
) : IRequest;