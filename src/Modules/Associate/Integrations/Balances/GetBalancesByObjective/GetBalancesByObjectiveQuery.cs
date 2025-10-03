using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Associate.Integrations.Balances.GetBalancesByObjective;

public sealed record GetBalancesByObjectiveQuery(
    string Entity,
    [property: HomologScope("TipoDocumento")] string DocumentType,
    string Identification,
    int PageNumber,
    int RecordsPerPage
) : IQuery<GetBalancesByObjectiveResponse>;
