using HotChocolate;

namespace Treasury.Presentation.GraphQL.Input;

public sealed record CreateTreasuryMovementInput(
    [property: GraphQLName("idPortafolio")] int PortfolioId,
    [property: GraphQLName("fechaCierre")] DateTime ClosingDate,
    [property: GraphQLName("concepto")] long TreasuryConceptId,
    [property: GraphQLName("valor")] decimal Value,
    [property: GraphQLName("fechaProceso")] DateTime ProcessDate,
    [property: GraphQLName("cuentaBancaria")] long BankAccountId,
    [property: GraphQLName("entidad")] long EntityId,
    [property: GraphQLName("contraparte")] long CounterpartyId
);
