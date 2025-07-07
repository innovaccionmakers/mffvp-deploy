using HotChocolate;
using System.Text.Json;

namespace Operations.Presentation.DTOs;

public record ContributionMutationResult(
    [property: GraphQLName("id")] long Id,
    [property: GraphQLName("etiquetaId")]  string LabelId,
    [property: GraphQLName("detalle")] JsonElement? Detail
);
