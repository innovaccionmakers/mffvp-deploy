using HotChocolate;

namespace Accounting.Presentation.ConfigurationGenerals.UpdateConfigurationGeneral
{
    public sealed record class UpdateConfigurationGeneralInput(
        [property: GraphQLName("portafolioId")]
        int PortfolioId,
        [property: GraphQLName("codigoContable")]
        string AccountingCode,
        [property: GraphQLName("centroCosto")]
        string CostCenter
    );
}
