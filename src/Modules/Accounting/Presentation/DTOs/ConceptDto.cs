using HotChocolate;

namespace Accounting.Presentation.DTOs
{
    public sealed record class ConceptDto(
        [property: GraphQLName("conceptoId")] long ConceptId,
        [property: GraphQLName("portafolioId")] int PortfolioId,
        [property: GraphQLName("nombre")] string Name,
        [property: GraphQLName("cuentaDebito")] string? DebitAccount,
        [property: GraphQLName("cuentaCredito")] string? CreditAccount
        );
}

