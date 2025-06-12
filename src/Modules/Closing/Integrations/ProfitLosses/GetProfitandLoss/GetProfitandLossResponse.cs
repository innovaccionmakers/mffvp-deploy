namespace Closing.Integrations.ProfitLosses.GetProfitandLoss;

public sealed record GetProfitandLossResponse(
    IReadOnlyDictionary<string, decimal> Values,
    decimal NetYield
);