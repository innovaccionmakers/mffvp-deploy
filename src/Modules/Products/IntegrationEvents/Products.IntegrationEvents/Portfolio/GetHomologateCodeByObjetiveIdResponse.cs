namespace Products.IntegrationEvents.Portfolio;

public sealed record GetHomologateCodeByObjetiveIdResponse(
    bool Succeeded,
    string HomologatedCode,
    string? Code,
    string? Message
);
