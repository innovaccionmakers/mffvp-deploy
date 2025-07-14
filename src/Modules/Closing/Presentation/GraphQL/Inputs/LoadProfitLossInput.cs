using HotChocolate;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Closing.Presentation.GraphQL.Inputs;

public class LoadProfitLossInput
{
    [GraphQLName("portafolioId")]
    [Required]
    public int PortfolioId { get; set; }

    [GraphQLName("fechaEfectiva")]
    [Required]
    public DateTime EffectiveDate { get; set; }

    [GraphQLName("montosConceptos")]
    public JsonElement ConceptAmounts { get; set; } = new();
}