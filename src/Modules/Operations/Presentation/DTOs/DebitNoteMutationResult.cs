using HotChocolate;

namespace Operations.Presentation.DTOs;

public record DebitNoteMutationResult(
    [property: GraphQLName("id")] long DebitNoteId,
    [property: GraphQLName("mensaje")] string Message,
    [property: GraphQLName("etiquetaId")] string TagId
);
