namespace DataSync.IntegrationEvents.TrustSync;

public sealed record TrustSyncPostRequest(int PortfolioId, DateTime ClosingDate);