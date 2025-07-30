using HotChocolate;

namespace Products.Presentation.DTOs;

public record AffiliateGoalDto(
    [property: GraphQLName("id")] int Id,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("tipoObjetivoId")] string IdType,
    [property: GraphQLName("tipoObjetivo")] string Type,
    [property: GraphQLName("tipoObjetivoCodigoHomologado")] string HomologatedCodeType,
    [property: GraphQLName("planId")] string IdPlan,
    [property: GraphQLName("plan")] string Plan,
    [property: GraphQLName("fondoId")] string IdFund,
    [property: GraphQLName("fondo")] string Fund,
    [property: GraphQLName("alternativaId")] string IdAlternative,
    [property: GraphQLName("alternativa")] string Alternative,
    [property: GraphQLName("comercialId")] string IdCommercial,
    [property: GraphQLName("comercial")] string Commercial,
    [property: GraphQLName("oficinaAperturaId")] string IdOpeningOffice,
    [property: GraphQLName("oficinaApertura")] string OpeningOffice,
    [property: GraphQLName("oficinaActualId")] string IdCurrentOffice,
    [property: GraphQLName("oficinaActual")] string CurrentOffice,
    [property: GraphQLName("estado")] string Status
);