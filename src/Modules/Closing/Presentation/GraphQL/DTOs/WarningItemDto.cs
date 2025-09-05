using HotChocolate;

namespace Closing.Presentation.GraphQL.DTOs;

public record WarningItemDto
  (
    [property: GraphQLName("Codigo")] string Code,
    [property: GraphQLName("Descripcion")] string Description,
    [property: GraphQLName("Severidad")] string Severity,
    [property: GraphQLName("Prioridad")] int Priority
  );

public static class WarningItemMapping
{
    public static WarningItemDto ToDto(this Closing.Integrations.Common.WarningItem w) =>
        new(w.Code, w.Description, w.Severity, w.Priority);
}
