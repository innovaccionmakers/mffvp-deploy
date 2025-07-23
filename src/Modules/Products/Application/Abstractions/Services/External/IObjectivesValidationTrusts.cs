using Common.SharedKernel.Domain;

namespace Products.Application.Abstractions.Services.External;

public interface IObjectivesValidationTrusts
{
    Task<Result<ObjectivesValidationData>> ValidateAsync(
        int ObjectiveId,
        string RequestedStatus,
        CancellationToken ct);
}

public readonly record struct ObjectivesValidationData(
    bool CanUpdate,
    bool HasTrust,
    bool HasTrustWithBalance,
    string? Code = null,
    string? Message = null
);
