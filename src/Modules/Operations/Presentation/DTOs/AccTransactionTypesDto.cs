using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain.OperationTypes;
using HotChocolate;
using System.Text.Json;

namespace Operations.Presentation.DTOs
{
    public  record class AccTransactionTypesDto(
        [property: GraphQLName("id")] long OperationTypeId,
        [property: GraphQLName("nombre")] string Name,
        [property: GraphQLName("categoria")] int? CategoryId,
        [property: GraphQLName("naturaleza")] IncomeEgressNature Nature,
        [property: GraphQLName("estado")] Status Status,
        [property: GraphQLName("externo")] string External,
        [property: GraphQLName("visible")] bool Visible,
        [property: GraphQLName("atributosAdicionales")] JsonDocument AdditionalAttributes,
        [property: GraphQLName("codigoHomologado")] string HomologatedCode
        );
}
