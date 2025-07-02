using HotChocolate;

namespace Products.Presentation.DTOs;

public record class GoalDto(
    [property: GraphQLName("id")] int Id,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("estado")] string Status
);
