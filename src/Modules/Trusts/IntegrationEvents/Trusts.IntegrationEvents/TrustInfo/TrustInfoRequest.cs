namespace Trusts.IntegrationEvents.TrustInfo;

public sealed record TrustInfoRequest(long ClientOperationId, decimal ContributionValue);
