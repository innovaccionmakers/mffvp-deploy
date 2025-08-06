using Common.SharedKernel.Domain;

namespace Operations.Application.Abstractions.External;

public interface ITrustCreator
{
    Task<Result> CreateAsync(TrustCreationDto dto, CancellationToken ct);
}

public sealed record TrustCreationDto(
    int AffiliateId,
    long ClientOperationId,
    DateTime CreationDate,
    int ObjectiveId,
    int PortfolioId,
    decimal TotalBalance,
    decimal TotalUnits,
    decimal Principal,
    decimal Earnings,
    int TaxCondition,
    decimal ContingentWithholding,
    decimal EarningsWithholding,
    decimal AvailableAmount);