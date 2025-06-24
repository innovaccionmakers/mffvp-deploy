namespace Operations.Presentation.DTOs;


[GraphQLName("MetodoPago")]
public record PaymentMethodDto(
    [property: GraphQLName("uuid")] string Uuid,
    [property: GraphQLName("nombre")] string Name,
    [property: GraphQLName("estado")] bool Status,
    [property: GraphQLName("codigoHomologacion")] string HomologationCode
);


