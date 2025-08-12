using HotChocolate;

namespace Associate.Presentation.GraphQL.Inputs;

public record CreateActivateInput(
    [property: GraphQLName("idTipoIdentificacion")] string DocumentType,
    [property: GraphQLName("identificacion")] string Identification,
    [property: GraphQLName("pensionado")] bool? Pensioner,
    [property: GraphQLName("cumpleRequisitosPension")] bool? MeetsPensionRequirements,
    [property: GraphQLName("fechaInicioReqPen")] DateOnly? StartDateReqPen,
    [property: GraphQLName("fechaFinReqPen")] DateOnly? EndDateReqPen
);
