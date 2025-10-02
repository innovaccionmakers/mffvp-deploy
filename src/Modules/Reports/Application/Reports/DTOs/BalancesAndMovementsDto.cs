namespace Reports.Application.Reports.DTOs
{
    public sealed record TrustYieldRequest(
        int PortfolioId,
        int ActivitesId,
        int ObjectsId,
        decimal Yields,
        decimal InitialBalance,
        decimal ClosingBalance
        );

    public sealed record PersonsRequest(
        int ActiviteId,
        string Identification,
        string IdentificationType,
        string FullName
        );

    public sealed record OperationBalancesRequest(
        int PortfolioId,
        int ActivitesId,
        int ObjectsId,
        decimal Entry
        );

    public sealed record OperationMovementsRequest(
        int PortfolioId,
        int ActiviteId,
        int ObjectId,
        long Voucher,
        DateTime ProcessDate,
        string TransactionType,
        string TransactionSubtype,
        decimal Value,
        string TaxCondition,
        decimal ContingentWithholding,
        string PaymentMethod
      );

    public sealed record ProductsRequest(
        int PortfolioId,
        int ObjectiveId,
        string Objective,
        string Fund,
        string Plan,
        string Alternative,
        string Portfolio
        );

    public sealed record AlternativeRequest(
        int AlternativeId
        );
}
