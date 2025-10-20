using System.Text.Json.Serialization;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.AccountingRecords.CreateDebitNote;

[AuditLog]
public sealed record AccountingRecordsValCommand(
    [property: JsonPropertyName("operacion_cliente_id")]
    long ClientOperationId,
    [property: JsonPropertyName("valor")]
    decimal Amount,
    [property: HomologScope("Causales Nota Débito")]
    [property: JsonPropertyName("causal_id")]
    int CauseId,
    [property: JsonPropertyName("afiliado_id")]
    int AffiliateId,
    [property: JsonPropertyName("objetivo_id")]
    int ObjectiveId
) : ICommand<AccountingRecordsValResult>;
