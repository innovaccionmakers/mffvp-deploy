using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs.AccountingInput
{
    public record AccountingInput(
        [property: GraphQLName("idsPortafolio")] IEnumerable<int> PortfolioIds,
        [property: GraphQLName("fechaProceso")] DateOnly ProcessDate
    );
}
