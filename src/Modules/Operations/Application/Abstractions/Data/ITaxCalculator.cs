namespace Operations.Application.Abstractions.Data;

public interface ITaxCalculator
{
    Task<TaxResult> ComputeAsync(bool isPensioner, bool isCertified, decimal amount, CancellationToken ct);
}

public readonly record struct TaxResult(
    int TaxConditionId,
    int CertificationStatusId,
    decimal WithheldAmount,
    string TaxConditionName);