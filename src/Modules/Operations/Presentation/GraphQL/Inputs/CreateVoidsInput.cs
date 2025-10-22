using System.Collections.Generic;
using HotChocolate;

namespace Operations.Presentation.GraphQL.Inputs;

public record VoidTransactionItemInput(
    [property: GraphQLName("operacionClienteId")] long ClientOperationId,
    [property: GraphQLName("valor")] decimal Amount);

public record CreateVoidsInput(
    [property: GraphQLName("transacciones")] IReadOnlyCollection<VoidTransactionItemInput> Items,
    [property: GraphQLName("causalId")] long CauseId,
    [property: GraphQLName("afiliadoId")] int AffiliateId,
    [property: GraphQLName("objetivoId")] int ObjectiveId);
