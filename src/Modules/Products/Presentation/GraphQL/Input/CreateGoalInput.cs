using Common.SharedKernel.Application.Attributes;
using HotChocolate;
using System.Text.Json.Serialization;

namespace Products.Presentation.GraphQL.Input;

public record CreateGoalInput(
    [property: GraphQLName("tipoId")]
    string IdType,

    [property: GraphQLName("identificacion")]
    string Identification,

    [property: GraphQLName("alternativaId")]
    string AlternativeId,

    [property: GraphQLName("tipoObjetivo")]
    string ObjectiveType,

    [property: GraphQLName("nombreObjetivo")]
    string ObjectiveName,

    [property: GraphQLName("oficinaApertura")]
    string OpeningOffice,

    [property: GraphQLName("oficinaActual")]
    string CurrentOffice,

    [property: GraphQLName("comercial")]
    string Commercial
);