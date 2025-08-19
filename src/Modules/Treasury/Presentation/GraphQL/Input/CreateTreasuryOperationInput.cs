using Common.SharedKernel.Domain;
using HotChocolate;

namespace Treasury.Presentation.GraphQL.Input;

public record TreasuryOperationInput(
    [property: GraphQLName("concepto")] string Concept,
    [property: GraphQLName("naturaleza")] IncomeExpenseNature Nature,
    [property: GraphQLName("admiteNegativo")] bool AllowsNegative,
    [property: GraphQLName("permiteGasto")] bool AllowsExpense,
    [property: GraphQLName("requiereCuentaBancaria")] bool RequiresBankAccount,
    [property: GraphQLName("requiereContraparte")] bool RequiresCounterparty,
    [property: GraphQLName("observaciones")] string? Observations,
    [property: GraphQLName("id")] long? Id = null
);
