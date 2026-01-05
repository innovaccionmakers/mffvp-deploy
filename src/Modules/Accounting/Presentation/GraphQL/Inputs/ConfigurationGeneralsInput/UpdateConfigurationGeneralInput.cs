using Common.SharedKernel.Infrastructure.Validation;
using HotChocolate;

namespace Accounting.Presentation.ConfigurationGenerals.UpdateConfigurationGeneral
{
    public sealed record class UpdateConfigurationGeneralInput(
        [property: GraphQLName("portafolioId")]
        int PortfolioId,

        [property: GraphQLName("codigoContable")]
        [property: MaxCharLength(10)]
        string AccountingCode,

        [property: GraphQLName("centroCosto")]
        [property: MaxCharLength(12)]
        string CostCenter
    );
}
