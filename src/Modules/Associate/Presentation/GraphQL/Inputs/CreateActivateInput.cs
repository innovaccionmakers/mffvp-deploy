using HotChocolate;

namespace Associate.Presentation.GraphQL.Inputs;

public record CreateActivateInput(
    [property: GraphQLName("tipoId")] string DocumentType,
    [property: GraphQLName("identificacion")] string Identification,
    [property: GraphQLName("pensionado")] bool? Pensioner,
    [property: GraphQLName("cumpleRequisitosPension")] bool? MeetsPensionRequirements,
    [property: GraphQLName("fechaInicioReqPen")] DateTime? StartDateReqPen,
    [property: GraphQLName("fechaFinReqPen")] DateTime? EndDateReqPen
);
