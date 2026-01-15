using Common.SharedKernel.Application.Messaging;
using Products.Integrations.Administrators;

namespace Products.Integrations.Administrators.GetFirstAdministrator;

public sealed record GetFirstAdministratorQuery() : IQuery<AdministratorResponse?>;

