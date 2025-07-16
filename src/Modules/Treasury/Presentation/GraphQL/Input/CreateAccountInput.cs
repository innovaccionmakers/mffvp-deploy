using HotChocolate;

namespace Treasury.Presentation.GraphQL.Input;

public sealed record CreateAccountInput(
    [property: GraphQLName("idPortafolio")] int PortfolioId,
    [property: GraphQLName("idEmisor")] int IssuerId,
    [property: GraphQLName("emisor")] string Issuer,
    [property: GraphQLName("numeroCuenta")] string AccountNumber,
    [property: GraphQLName("tipoCuenta")] string AccountType,
    [property: GraphQLName("observaciones")] string? Observations
);
