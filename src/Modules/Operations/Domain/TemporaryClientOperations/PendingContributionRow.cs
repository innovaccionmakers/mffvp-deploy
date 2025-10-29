using Common.SharedKernel.Core.Primitives;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Operations.Domain.TemporaryClientOperations;

[Keyless]
public sealed class PendingContributionRow
{
    public long TemporaryClientOperationId { get; init; }
    public DateTime RegistrationDate { get; init; }
    public int AffiliateId { get; init; }
    public int ObjectiveId { get; init; }
    public int PortfolioId { get; init; }
    public decimal Amount { get; init; }
    public DateTime ProcessDate { get; init; }
    public long OperationTypeId { get; init; }
    public DateTime ApplicationDate { get; init; }
    public bool Processed { get; init; }
    public long? TrustId { get; init; }
    public long? LinkedClientOperationId { get; init; }
    public LifecycleStatus Status { get; init; }
    public decimal? Units { get; init; }
    public int? CauseId { get; init; }

    public long TemporaryAuxiliaryInformationId { get; init; }
    public int OriginId { get; init; }
    public int CollectionMethodId { get; init; }
    public int PaymentMethodId { get; init; }
    public string? CollectionAccount { get; init; }
    public JsonDocument PaymentMethodDetail { get; init; }
    public int CertificationStatusId { get; init; }
    public int TaxConditionId { get; init; }
    public decimal ContingentWithholding { get; init; }
    public JsonDocument VerifiableMedium { get; init; }
    public int CollectionBankId { get; init; }
    public DateTime DepositDate { get; init; }
    public string? SalesUser { get; init; }
    public int OriginModalityId { get; init; }
    public int CityId { get; init; }
    public int ChannelId { get; init; }
    public string? UserId { get; init; }
}