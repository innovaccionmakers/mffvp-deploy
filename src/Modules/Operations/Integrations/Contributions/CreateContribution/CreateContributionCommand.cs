using System.Text.Json;
using System.Text.Json.Serialization;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Operations.Integrations.Contributions.CreateContribution;

public sealed record CreateContributionCommand(
    [property: JsonPropertyName("TipoId")]
    string TypeId,

    [property: JsonPropertyName("Identificacion")]
    string Identification,

    [property: JsonPropertyName("IdObjetivo")]
    int ObjectiveId,

    [property: JsonPropertyName("IdPortafolio")]
    string? PortfolioId,

    [property: JsonPropertyName("Valor")]
    decimal Amount,

    [property: JsonPropertyName("Origen")]
    [property: HomologScope("Origen Aporte")]
    string Origin,

    [property: JsonPropertyName("ModalidadOrigen")]
    [property: HomologScope("Modalidad Origen Aporte")]
    string OriginModality,

    [property: JsonPropertyName("MetodoRecaudo")]
    [property: HomologScope("Metodo de Recaudo")]
    string CollectionMethod,

    [property: JsonPropertyName("FormaPago")]
    [property: HomologScope("Metodo de Pago")]
    string PaymentMethod,

    [property: JsonPropertyName("DetalleFormaPago")]
    JsonDocument? PaymentMethodDetail,

    [property: JsonPropertyName("BancoRecaudo")]
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
    JsonDocument? VerifiableMedium
) : ICommand<ContributionResponse>;