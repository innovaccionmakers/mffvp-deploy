namespace Associate.Domain.Clients;

public sealed record Client(
    string IdType,
    string IdNumber,
    bool IsActive,
    bool IsBlocked,
    bool ProductActivated);