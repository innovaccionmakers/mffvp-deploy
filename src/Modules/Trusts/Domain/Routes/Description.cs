namespace Trusts.Domain.Routes;

public struct Description
{
    public const string
        CreateTrust = """
                             **Ejemplo de petición (application/json):**
                             ```json
                             {
                               \"AffiliateId\": 1,
                               \"ClientOperationId\": 2,
                               \"CreationDate\": \"2025-06-01T00:00:00Z\",
                               \"ObjectiveId\": 123,
                               \"PortfolioId\": 456,
                               \"TotalBalance\": 100.5,
                               \"TotalUnits\": 10,
                               \"Principal\": 100,
                               \"Earnings\": 0.5,
                               \"TaxCondition\": 1,
                               \"ContingentWithholding\": 0,
                               \"EarningsWithholding\": 0,
                               \"AvailableAmount\": 100,
                               \"AccumulatedEarnings\": 0,
                               \"Status\": true
                             }
                             ```
                             """,
        GetBalances = """
                             **Ejemplo de llamada:**

                             ```http
                             GET /FVP/trusts/balances?affiliateId=1
                             ```
                             """,
        TrustSync = """
                             **Ejemplo de petición (application/json):**
                             ```json
                             {
                               \"closingDate\": \"2025-06-01\"
                             }
                             ```
                             """;
}

public struct RequestBodyDescription
{
    public const string
        CreateTrust = "",
        TrustSync = "";
}
