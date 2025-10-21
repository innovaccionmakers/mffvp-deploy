using HotChocolate;

namespace Operations.Presentation.GraphQL.Inputs;

public record CreateDebitNoteInput
{
    [GraphQLName("operacionClienteId")]
    public required long ClientOperationId { get; set; }

    [GraphQLName("valor")]
    public required decimal Amount { get; set; }

    [GraphQLName("causalId")]
    public required int CauseId { get; set; }

    [GraphQLName("afiliadoId")]
    public required int AffiliateId { get; set; }

    [GraphQLName("objetivoId")]
    public required int ObjectiveId { get; set; }
}
