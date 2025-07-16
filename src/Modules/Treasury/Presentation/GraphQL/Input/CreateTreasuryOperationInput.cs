using Common.SharedKernel.Domain;
using HotChocolate;

namespace Treasury.Presentation.GraphQL.Input;

public record CreateTreasuryOperationInput(
    [property: GraphQLName("concepto")] string Concept,
    [property: GraphQLName("naturaleza")] IncomeExpenseNature Nature,
    [property: GraphQLName("admiteNegativo")] bool AllowsNegative,
    [property: GraphQLName("permiteGasto")] bool AllowsExpense,
    [property: GraphQLName("RequiereCuentaBancaria")] bool RequiresBankAccount,
    [property: GraphQLName("RequiereContraparte")] bool RequiresCounterparty,
    [property: GraphQLName("fechaProceso")] DateTime ProcessDate,
    [property: GraphQLName("observaciones")] string? Observations
);
