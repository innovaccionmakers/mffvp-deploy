
namespace Closing.Domain.Yields;
public sealed record YieldToDistributeDto(
    decimal YieldToCredit, decimal Income, decimal Expenses, decimal Commissions, decimal Costs);