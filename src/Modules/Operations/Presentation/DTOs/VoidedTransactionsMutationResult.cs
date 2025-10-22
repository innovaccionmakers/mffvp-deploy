using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using Operations.Integrations.Voids.RegisterVoidedTransactions;

namespace Operations.Presentation.DTOs;

public record VoidedTransactionFailureDto(
    [property: GraphQLName("operacionClienteId")] long ClientOperationId,
    [property: GraphQLName("codigo")] string Code,
    [property: GraphQLName("mensaje")] string Message);

public record VoidedTransactionsMutationResult(
    [property: GraphQLName("idsAnulacion")] IReadOnlyCollection<long> VoidIds,
    [property: GraphQLName("mensaje")] string Message,
    [property: GraphQLName("operacionesFallidas")] IReadOnlyCollection<VoidedTransactionFailureDto> FailedOperations)
{
    public static VoidedTransactionsMutationResult FromResult(
        VoidedTransactionsValResult result)
    {
        var failures = result.FailedOperations
            .Select(failure => new VoidedTransactionFailureDto(
                failure.ClientOperationId,
                failure.Code,
                failure.Message))
            .ToArray();

        return new VoidedTransactionsMutationResult(result.VoidIds, result.Message, failures);
    }
}
