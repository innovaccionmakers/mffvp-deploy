namespace Customers.Integrations.Municipalities;

public sealed record MunicipalityResponse(
    int MunicipalityId,
    int CityCode,
    string Name,
    int DialingCode,
    int DaneCode,
    string HomologatedCode
);