namespace Reports.Domain.LoadingInfo.Products;

public sealed record ProductSheetReadRow(
    int AdministratorId,
    int EntityType,
    string EntityCode,
    string EntitySfcCode,
    int BusinessCodeSfcFund,
    int FundId
);
