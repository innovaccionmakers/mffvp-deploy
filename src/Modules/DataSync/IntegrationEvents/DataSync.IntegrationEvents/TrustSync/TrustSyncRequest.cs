namespace DataSync.IntegrationEvents.TrustSync;

public sealed record TrustSyncRequest(DateTime ClosingDate, int PortfolioId);
