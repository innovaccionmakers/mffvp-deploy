namespace Closing.IntegrationEvents.DataSync.TrustSync;

public sealed record TrustSyncRequest(
    int TrustId,
    int PortfolioId,
    DateTime ClosingDate,
    decimal PreClosingBalance,
    decimal Capital,
    decimal ContingentWithholding);
