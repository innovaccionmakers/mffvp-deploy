using HotChocolate;

namespace Accounting.Presentation.DTOs
{
    public sealed record class ConfigurationGeneralDto(
        [property: GraphQLName("id")]
        long Id,
        [property: GraphQLName("codigoContable")]
        string AccountingCode,
        [property: GraphQLName("centroCosto")]
        string CostCenter
    );
}
