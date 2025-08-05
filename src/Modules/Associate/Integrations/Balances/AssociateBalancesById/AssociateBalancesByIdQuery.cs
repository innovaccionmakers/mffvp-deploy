using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.Balances.AssociateBalancesById;

[AuditLog]
public sealed record AssociateBalancesByIdQuery(
    [property: HomologScope("TipoDocumento")]
    string DocumentType,
    string Identification
    ) : IQuery<IReadOnlyCollection<AssociateBalanceItem>>;
