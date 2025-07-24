using HotChocolate;

namespace Products.Presentation.GraphQL.Input;

public record UpdateGoalInput
(
    [property: GraphQLName("idObjetivo")]
    int ObjectiveId,

    [property: GraphQLName("tipoObjetivo")]
    string ObjectiveType,

    [property: GraphQLName("nombreObjetivo")]
    string ObjectiveName,

    [property: GraphQLName("oficinaApertura")]
    string OpeningOffice,

    [property: GraphQLName("oficinaActual")]
    string CurrentOffice,

    [property: GraphQLName("comercial")]
    string Commercial,

    [property: GraphQLName("estado")]
    string Status
);