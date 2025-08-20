using HotChocolate;
using System.Text.Json;

namespace Operations.Presentation.GraphQL.Inputs;

public record CreateContributionInput
{
    [GraphQLName("idTipoIdentificacion")]
    public required string TypeId { get; set; }

    [GraphQLName("identificacion")]
    public required string Identification { get; set; }

    [GraphQLName("idObjetivo")]
    public required int ObjectiveId { get; set; }

    [GraphQLName("idPortafolio")]
    public string? PortfolioId { get; set; }
    
    [GraphQLName("valor")]
    public required decimal Amount { get; set; }
    
    [GraphQLName("origen")]
    public required string Origin { get; set; }
    
    [GraphQLName("modalidadOrigen")]
    public required string OriginModality { get; set; }
    
    [GraphQLName("metodoRecaudo")]
    public required string CollectionMethod { get; set; }
    
    [GraphQLName("formaPago")]
    public required string PaymentMethod { get; set; }

    [GraphQLName("detalleFormaPago")]
    public JsonElement? PaymentMethodDetail { get; set; }
    
    [GraphQLName("bancoRecaudo")]
    public required string CollectionBank { get; set; }
    
    [GraphQLName("cuentaRecaudo")]
    public required string CollectionAccount { get; set; }
    
    [GraphQLName("aporteCertificado")]
    public string? CertifiedContribution { get; set; }

    [GraphQLName("retencionContingente")]
    public decimal? ContingentWithholding { get; set; }
    
    [GraphQLName("fechaConsignacion")]
    public required DateTime DepositDate { get; set; }
    
    [GraphQLName("fechaEjecucion")]
    public DateTime? ExecutionDate { get; set; }
    
    [GraphQLName("usuarioComercial")]
    public string? SalesUser { get; set; }

    [GraphQLName("medioVerificable")]
    public JsonElement? VerifiableMedium { get; set; }

    [GraphQLName("subtipo")]
    public string? Subtype { get; set; }
    
    [GraphQLName("canal")]
    public string? Channel { get; set; }
    
    [GraphQLName("usuario")]
    public required string User { get; set; }

}