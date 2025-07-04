using HotChocolate;

namespace Operations.Presentation.GraphQL.Inputs;

public record PaymentMethodDetail
{
    [GraphQLName("numeroTarjeta")]
    public string? CardNumber { get; set; }

    [GraphQLName("expiracion")]
    public string? Expiry { get; set; }
}