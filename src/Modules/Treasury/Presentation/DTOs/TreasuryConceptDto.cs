using HotChocolate;

namespace Treasury.Presentation.DTOs;

public record TreasuryConceptDto(
    [property: GraphQLName("id")] long Id,
    [property: GraphQLName("concepto")] string Concept,
    [property: GraphQLName("naturaleza")] string Nature,
    [property: GraphQLName("permiteNegativo")] string AllowsNegative,
    [property: GraphQLName("permiteGasto")] string AllowsExpense,
    [property: GraphQLName("requiereCuentaBancaria")] string RequiresBankAccount,
    [property: GraphQLName("requiereContraparte")] string RequiresCounterparty,
    [property: GraphQLName("fechaProceso")] DateTime ProcessDate,
    [property: GraphQLName("observaciones")] string Observations
);