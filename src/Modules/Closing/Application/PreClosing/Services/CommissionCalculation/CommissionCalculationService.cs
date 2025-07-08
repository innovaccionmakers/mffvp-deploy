using Closing.Application.Abstractions.External.Commissions;
using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.CommissionCalculation;

public class CommissionCalculationService : ICommissionCalculationService
{
    private readonly ICommissionLocator _commissionLocator;

    public CommissionCalculationService(ICommissionLocator commissionLocator)
    {
        _commissionLocator = commissionLocator;
    }

    public async Task<Result<decimal>> CalculateAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        var commissionsResult = await _commissionLocator.GetActiveCommissionsAsync(portfolioId, ct);

        return commissionsResult.Match(
            commissions =>
            {
                // Lógica de cálculo de comisiones basada en los datos
                decimal total = commissions.Sum(c => 0m);
                return Result.Success(total);
            },
            error => Result.Failure<decimal>(error)
        );
    }
}