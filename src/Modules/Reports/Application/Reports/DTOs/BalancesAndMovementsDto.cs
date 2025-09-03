namespace Reports.Application.Reports.DTOs
{
    public sealed record ActivateRequest(
        string Identification,
        int ActivitesId
        );

    public sealed record TrustYieldRequest(
        long TrustYieldId,
        int ActivitesId,
        int ObjectsId
        );

    public sealed record PersonsRequest(
        string Identification,
        string IdentificationType,
        string FullName
        );

    public sealed record OperationRequest(
        int PortfolioId,
        decimal Entry
        );

    public sealed record CloseRequest(
        long TrustYieldId,
        decimal InitialBalance,
        decimal Yields
        );

    public sealed record ProductsRequest(
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
