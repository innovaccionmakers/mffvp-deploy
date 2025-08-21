namespace DataSync.Application.TrustSync.Dto;
public sealed record TrustRow(
    long TrustId,
    int PortfolioId,
    DateTime ClosingDate,
    decimal PreClosingBalance,
    decimal Capital,
    decimal ContingentRetention
);