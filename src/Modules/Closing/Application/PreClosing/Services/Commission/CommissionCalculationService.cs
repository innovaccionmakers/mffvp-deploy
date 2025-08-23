using Closing.Application.Abstractions.External.Products.Commissions;
using Closing.Application.PreClosing.Services.Commission.Constants;
using Closing.Application.PreClosing.Services.Commission.Dto;
using Closing.Application.PreClosing.Services.Commission.Interfaces;
using System.Globalization;

namespace Closing.Application.PreClosing.Services.Commission;

public class CommissionCalculationService : ICommissionCalculationService
{
    private readonly ICommissionLocator _commissionLocator;
    private readonly ICommissionAdminCalculation _commissionAdminCalculationService;

    public CommissionCalculationService(ICommissionLocator commissionLocator, ICommissionAdminCalculation commissionAdminCalculationService)
    {
        _commissionLocator = commissionLocator;
        _commissionAdminCalculationService = commissionAdminCalculationService;
    }

    public async Task<IReadOnlyList<CommissionConceptSummary>> CalculateAsync(
        int portfolioId,
        DateTime closingDate,
        CancellationToken cancellationToken = default)
    {
        var commissionsResult = await _commissionLocator.GetActiveCommissionsAsync(portfolioId, cancellationToken);
        var commissions = commissionsResult.Value;

        var summaries = new List<CommissionConceptSummary>();

        foreach (var commission in commissions)
        {
            if (!IsAdministrative(commission))
                continue;

            var percentage = ParseCommissionPercentage(commission.CalculationRule);
            var amount = await CalculateCommissionAmountAsync(portfolioId, closingDate, percentage, cancellationToken);

            summaries.Add(new CommissionConceptSummary(
                commission.CommissionId,
                commission.Concept,
                amount));
        }

        return summaries;
    }

    private static bool IsAdministrative(CommissionsByPortfolioRemoteResponse commission) =>
    commission.Concept.Equals(CommissionConcepts.Administrative, StringComparison.OrdinalIgnoreCase);

    private static decimal ParseCommissionPercentage(string rule)
    {
        if (!decimal.TryParse(rule, NumberStyles.Any, CultureInfo.InvariantCulture, out var percentage))
            throw new InvalidOperationException($"No se pudo convertir '{rule}' a decimal.");

        return percentage;
    }

    private async Task<decimal> CalculateCommissionAmountAsync(
        int portfolioId,
        DateTime closingDate,
        decimal percentage,
        CancellationToken cancellationToken = default)
    {
        var result = await _commissionAdminCalculationService.CalculateAsync(
            portfolioId, closingDate, percentage, cancellationToken);

        return result.IsSuccess ? result.Value : 0m;
    }

}