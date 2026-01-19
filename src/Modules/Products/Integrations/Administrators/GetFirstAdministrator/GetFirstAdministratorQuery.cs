using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Administrators.GetFirstAdministrator;

public sealed record GetFirstAdministratorQuery() : IQuery<AdministratorResponse?>;

