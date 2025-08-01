using HotChocolate;

namespace Treasury.Presentation.DTOs;

public record BankAccountByPortfolioDto(
    [property: GraphQLName("cuentaId")] long Id,
    [property: GraphQLName("portafolioId")] long PortfolioId,
    [property: GraphQLName("nombreCortoPortafolio")] string PortfolioShortName,
    [property: GraphQLName("emisorId")] long IssuerId,
    [property: GraphQLName("emisor")] string IssuerName,
    [property: GraphQLName("emisorDescripcion")] string IssuerDescription,
    [property: GraphQLName("numeroCuenta")] string AccountNumber,
    [property: GraphQLName("tipoCuenta")] string AccountType,
    [property: GraphQLName("observaciones")] string Observations,
    [property: GraphQLName("fechaProceso")] DateTime ProcessDate
);