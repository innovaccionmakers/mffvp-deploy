namespace People.Integrations.EconomicActivities;

public sealed record EconomicActivityResponse(
    string EconomicActivityId,
    string Description,
    string CiiuCode,
    string DivisionCode,
    string DivisionName,
    string GroupName,
    string ClassCode,
    string StandardCode
);