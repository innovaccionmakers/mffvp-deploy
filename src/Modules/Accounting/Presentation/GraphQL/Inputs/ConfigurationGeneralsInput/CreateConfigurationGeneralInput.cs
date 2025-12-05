using HotChocolate;

namespace Accounting.Presentation.ConfigurationGenerals.CreateConfigurationGeneral
{
    public sealed record class CreateConfigurationGeneralInput(
        [property: GraphQLName("portafolioId")]
        int PortfolioId,
        [property: GraphQLName("codigoContable")]
        string AccountingCode,
        [property: GraphQLName("centroCosto")]
        string CostCenter
    );
}
