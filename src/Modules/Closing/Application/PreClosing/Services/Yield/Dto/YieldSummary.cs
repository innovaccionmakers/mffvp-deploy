namespace Closing.Application.PreClosing.Services.Yield.Dto;
public record YieldSummary(decimal Income, decimal Expenses, decimal Commissions)
{
    public decimal Costs => Expenses + Commissions;
    public decimal YieldToCredit => Income - Costs;
}
