using System.Text.Json;
using System.Text.Json.Serialization;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.Contributions.CreateContribution;

public sealed record CreateContributionCommand(
    [property: JsonPropertyName("TipoId")]
    [property: HomologScope("TipoDocumento")]
    string TypeId,
    [property: JsonPropertyName("Identificacion")]
    string Identification,
    [property: JsonPropertyName("IdObjetivo")]
    int ObjectiveId,
    [property: JsonPropertyName("IdPortafolio")]
    string? PortfolioId,
    [property: JsonPropertyName("Valor")] decimal Amount,
    [property: JsonPropertyName("Origen")]
    [property: HomologScope("Origen Aporte")]
    string Origin,
    [property: JsonPropertyName("ModalidadOrigen")]
    [property: HomologScope("ModalidadOrigen")]
    string OriginModality,
    [property: JsonPropertyName("MetodoRecaudo")]
    [property: HomologScope("MetodoRecaudo")]
    string CollectionMethod,
    [property: JsonPropertyName("FormaPago")]
    [property: HomologScope("FormaPago")]
    string PaymentMethod,
    [property: JsonPropertyName("DetalleFormaPago")]
    JsonDocument? PaymentMethodDetail,
    [property: JsonPropertyName("BancoRecaudo")]
    [property: HomologScope("BancoRecaudo")]
    string CollectionBank,
    [property: JsonPropertyName("CuentaRecaudo")]
    string CollectionAccount,
    [property: JsonPropertyName("AporteCertificado")]
    string? CertifiedContribution,
    [property: JsonPropertyName("RetencionContingente")]
    decimal? ContingentWithholding,
    [property: JsonPropertyName("FechaConsignacion")]
    DateTime DepositDate,
    [property: JsonPropertyName("FechaEjecucion")]
    DateTime ExecutionDate,
    [property: JsonPropertyName("UsuarioComercial")]
    string SalesUser,
    [property: JsonPropertyName("MedioVerificable")]
    JsonDocument? VerifiableMedium,
    [property: JsonPropertyName("Subtipo")]
    string? Subtype,
    [property: JsonPropertyName("Canal")] string Channel,
    [property: JsonPropertyName("Usuario")]
    string User
) : ICommand<ContributionResponse>;