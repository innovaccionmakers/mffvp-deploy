using HotChocolate;

namespace Accounting.Presentation.ConfigurationGenerals.DeleteConfigurationGeneral
{
    public sealed record class DeleteConfigurationGeneralInput(
        [property: GraphQLName("portafolioId")]
        int PortfolioId
    );
}
