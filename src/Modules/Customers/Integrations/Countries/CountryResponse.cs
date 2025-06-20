namespace Customers.Integrations.Countries;

public sealed record CountryResponse(
    int CountryId,
    string Name,
    string ShortName,
    int DaneCode,
    string HomologatedCode
);