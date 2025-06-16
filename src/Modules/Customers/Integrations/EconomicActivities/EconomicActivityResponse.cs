namespace Customers.Integrations.EconomicActivities;

public sealed record EconomicActivityResponse(
    int EconomicActivityId,
    string GroupCode,
    string Description,
    string CiiuCode,
    string DivisionCode,
    string DivisionName,
    string GroupName,
    string ClassCode,
    string HomologatedCode
);