namespace Reports.Domain.LoadingInfo.Constants;

[Flags]
public enum EtlSelection
{
    None = 0,

    AffiliatesClients = 1 << 0,
    Balances = 1 << 1,
    Closing = 1 << 2,
    Products = 1 << 3,

    All = AffiliatesClients | Balances | Closing | Products
}


