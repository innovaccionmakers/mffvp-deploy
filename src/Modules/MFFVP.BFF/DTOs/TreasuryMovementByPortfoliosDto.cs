namespace MFFVP.BFF.DTOs;

public sealed record TreasuryMovementByPortfoliosDto(
    [property: GraphQLName("portafolio")] string Portfolio,
    [property: GraphQLName("fechaCierre")] DateOnly ClosingDate,
    [property: GraphQLName("concepto")] string Concept,
    [property: GraphQLName("distribucion")] string Distribution,
    [property: GraphQLName("tipoConcepto")] string TypeConcept,
    [property: GraphQLName("cuentaBancaria")] string? BankAccount,
    [property: GraphQLName("contraparte")] string? Counterpart,
    [property: GraphQLName("valor")] decimal Value
);
