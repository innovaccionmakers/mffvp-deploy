using HotChocolate;
using System.Text.Json;

namespace Products.Presentation.DTOs;

public record GoalMutationResult(
    [property: GraphQLName("id")] long Id,
    [property: GraphQLName("etiquetaId")]  string LabelId,
    [property: GraphQLName("detalle")] JsonElement? Detail
);
