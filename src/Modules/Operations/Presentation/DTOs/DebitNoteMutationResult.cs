using HotChocolate;

namespace Operations.Presentation.DTOs;

public record DebitNoteMutationResult(
    [property: GraphQLName("idNotaDebito")] long DebitNoteId,
    [property: GraphQLName("mensaje")] string Message
);
