namespace Closing.IntegrationEvents.DataSync.TrustSync;

public sealed record TrustSyncPostRequest(int PortfolioId,
                                            DateTime ClosingDate);
