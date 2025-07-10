using Closing.Application.Abstractions.External.Commissions;
using Closing.Domain.Commission;
using Closing.Domain.Constants;
using Closing.Domain.ProfitLosses;
using Common.SharedKernel.Domain;
using System.Globalization;

namespace Closing.Application.PreClosing.Services.CommissionCalculation;

public class CommissionCalculationService : ICommissionCalculationService
{
    private readonly ICommissionLocator _commissionLocator;
    private readonly ICommissionAdminCalculationService _commissionAdminCalculationService;

    public CommissionCalculationService(ICommissionLocator commissionLocator, ICommissionAdminCalculationService commissionAdminCalculationService)
    {
        _commissionLocator = commissionLocator;
        _commissionAdminCalculationService = commissionAdminCalculationService;
    }

    public async Task<IReadOnlyList<CommissionConceptSummary>> CalculateAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        var commissionsResult = await _commissionLocator.GetActiveCommissionsAsync(portfolioId, ct);
        var commissions = commissionsResult.Value;
        var commissionSummary = new List<CommissionConceptSummary>();
        foreach (var commission in commissions)
        {
            if (commission.Concept.Equals(CommissionConcepts.Administrative))
            {
                decimal commissionPercentage;
                if (!decimal.TryParse(commission.CalculationRule, NumberStyles.Any, CultureInfo.InvariantCulture, out commissionPercentage))
                {
                    // Manejar error de conversión
                    throw new InvalidOperationException($"No se pudo convertir '{commission.CalculationRule}' a decimal.");
                }

                var commissionAmountResult = await _commissionAdminCalculationService
                                            .CalculateAsync(portfolioId, closingDate, commissionPercentage, ct);

                decimal commissionAmount = commissionAmountResult.IsSuccess
                    ? commissionAmountResult.Value
                    : 0m;

                commissionSummary.Add(new CommissionConceptSummary(
                commission.CommissionId,
                commission.Concept,
                commissionAmount));
                        }
        }
        return commissionSummary;
    }
}