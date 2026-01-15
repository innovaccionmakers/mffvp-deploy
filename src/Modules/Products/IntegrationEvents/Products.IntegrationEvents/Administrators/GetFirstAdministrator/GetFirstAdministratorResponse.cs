using Products.Integrations.Administrators;

namespace Products.IntegrationEvents.Administrators.GetFirstAdministrator;

public sealed record GetFirstAdministratorResponse(
    bool Succeeded,
    AdministratorResponse? Administrator,
    string? Code,
    string? Message
);

