using Closing.Domain.ProfitLossConcepts;

namespace Closing.Domain.ProfitLosses;

public sealed record ProfitLossSummary(string Concept, ProfitLossNature Nature, decimal TotalAmount);