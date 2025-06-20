using Associate.Infrastructure.Database;
using Common.SharedKernel.Presentation.GraphQL;
using Customers.Infrastructure.Database;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

namespace MFFVP.Api.GraphQL;

[ExtendObjectType(nameof(RootQueryGraphQL))]
public class QuerySaraza
{
    public async Task<List<SarazaDto>> GetSarazasAsync(
        [Service] AssociateDbContext associateDbContext,
        [Service] CustomersDbContext customersDbContext,
        CancellationToken cancellationToken = default)
    {
        // Consulta los primeros 10 registros de Activates y Customers
        var activates = await associateDbContext.Activates
            .AsNoTracking()
            .Take(10)
            .ToListAsync(cancellationToken);

        var customers = await customersDbContext.Customers
            .AsNoTracking()
            .Take(10)
            .ToListAsync(cancellationToken);

        // Une los resultados por identificación (si existe coincidencia)
        var result = (from a in activates
                      join c in customers on a.Identification equals c.Identification into gj
                      from c in gj.DefaultIfEmpty()
                      select new SarazaDto(
                          a.DocumentType.ToString(),
                          a.Identification,
                          c?.GetFullName() ?? "Sin nombre asociado"
                      )).ToList();

        return result;
    }

}

public record SarazaDto(
    string IdentificationType,
    string Identification,
    string FullName
);
