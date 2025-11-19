using System;
using System.Threading;
using System.Threading.Tasks;

namespace Reports.Domain.TransmissionFormat;

public interface ITransmissionFormatReportRepository
{
    Task<Rt1Header> GetRt1HeaderAsync(int portfolioId, CancellationToken cancellationToken);
    Task<Rt2Header> GetRt2HeaderAsync(int portfolioId, CancellationToken cancellationToken);
    Task<decimal> GetUnitValueAsync(int portfolioId, DateTime date, CancellationToken cancellationToken);
    Task<Rt4ValuationMovements> GetValuationMovementsAsync(int portfolioId, DateTime date, CancellationToken cancellationToken);
    Task<Rt4Profitabilities> GetProfitabilitiesAsync(int portfolioId, DateTime date, CancellationToken cancellationToken);
    Task<AutomaticConceptAmounts> GetAutomaticConceptAmountsAsync(int portfolioId, DateTime date, CancellationToken cancellationToken);

    // New requirements
    Task<bool> AnyPortfolioExistsOnOrAfterDateAsync(DateTime date, CancellationToken cancellationToken);
    Task<IReadOnlyList<int>> GetPortfolioIdsWithClosureOnDateAsync(DateTime date, CancellationToken cancellationToken);
}

public sealed record Rt4ValuationMovements(
    decimal PreviousUnits,
    decimal PreviousAmount,
    decimal YieldAmount,
    decimal ContributionUnits,
    decimal ContributionAmount,
    decimal TransferUnits,
    decimal TransferAmount,
    decimal PensionUnits,
    decimal PensionAmount,
    decimal WithdrawalUnits,
    decimal WithdrawalAmount,
    decimal OtherCommissionUnits,
    decimal OtherCommissionAmount,
    decimal VitalityTransferUnits,
    decimal VitalityTransferAmount,
    decimal OtherWithdrawalUnits,
    decimal OtherWithdrawalAmount,
    decimal CancellationUnits,
    decimal CancellationAmount,
    decimal CurrentUnits,
    decimal CurrentAmount);

public sealed record Rt4Profitabilities(
    decimal ThirtyDay,
    decimal OneHundredEightyDay,
    decimal ThreeHundredSixtyFiveDay);

public sealed record AutomaticConceptAmounts(
    decimal AmountToBePaid,
    decimal AmountPaid,
    decimal TotalToBeDistributed,
    decimal PositiveToBeDistributed,
    decimal NegativeToBeDistributed);
