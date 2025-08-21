namespace DataSync.IntegrationEvents.TrustSync;

public sealed record TrustSyncRequest(int PortfolioId, DateTime ClosingDate);
