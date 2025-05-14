namespace People.Integrations.Countries;

public sealed record CountryResponse(
    int CountryId,
    string Name,
    string ShortName,
    string DaneCode,
    string StandardCode
);