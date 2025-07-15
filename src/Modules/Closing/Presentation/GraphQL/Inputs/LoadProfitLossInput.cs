using HotChocolate;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Closing.Presentation.GraphQL.Inputs;

public record LoadProfitLossInput(
    [property: GraphQLName("idPortafolio")] int PortfolioId,

    [property: GraphQLName("fechaEfectiva")] DateTime EffectiveDate,

    [property: GraphQLName("montosConceptos")] Dictionary<string, decimal> ConceptAmounts
);