
namespace Closing.Domain.Yields;
public sealed record YieldToDistribute(
    decimal YieldToCredit, decimal Income, decimal Expenses, decimal Commissions, decimal Costs);