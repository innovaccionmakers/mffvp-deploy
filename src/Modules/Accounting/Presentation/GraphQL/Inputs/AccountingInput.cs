using HotChocolate;

namespace Accounting.Presentation.GraphQL.Inputs
{
    public record AccountingInput(
        [property: GraphQLName("idsPortafolio")] List<int> PortfolioIds,
        [property: GraphQLName("fechaProceso")] DateTime ProcessDate
        );
}
