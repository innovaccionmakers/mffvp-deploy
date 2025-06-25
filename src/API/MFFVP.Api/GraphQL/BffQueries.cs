using Associate.Infrastructure.Database;
using Common.SharedKernel.Presentation.GraphQL;
using Customers.Infrastructure.Database;
using HotChocolate;
using HotChocolate.Types;
using MFFVP.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MFFVP.Api.GraphQL;

[ExtendObjectType(nameof(RootQueryGraphQL))]
public class BffQueries
{
    public async Task<List<AssociateDto>> GetAssociateAsync(
        string? tipoIdentificacion = null,
        string? searchBy = null,
        string? text = null,
        [Service] AssociateDbContext associateDbContext = null!,
        [Service] CustomersDbContext customersDbContext = null!,
        CancellationToken cancellationToken = default)
    {
        // Primero: Consulta y filtra Customers según los criterios de búsqueda
        var customersQuery = customersDbContext.Customers.AsNoTracking();

        // Aplicar filtro por tipo de identificación si se proporciona
        if (!string.IsNullOrWhiteSpace(tipoIdentificacion))
        {
            if (Guid.TryParse(tipoIdentificacion, out var documentTypeGuid))
            {
                customersQuery = customersQuery.Where(c => c.DocumentType == documentTypeGuid);
            }
        }

        // Aplicar filtros de búsqueda por texto si se proporcionan searchBy y text
        if (!string.IsNullOrWhiteSpace(searchBy) && !string.IsNullOrWhiteSpace(text))
        {
            var searchText = $"%{text}%"; // Agregar wildcards para búsqueda LIKE

            switch (searchBy.ToLower())
            {
                case "nombre":
                    customersQuery = customersQuery.Where(c =>
                        EF.Functions.ILike(c.FirstName, searchText) ||
                        EF.Functions.ILike(c.LastName, searchText) ||
                        EF.Functions.ILike(c.FullName, searchText));
                    break;
                case "identificacion":
                    customersQuery = customersQuery.Where(c =>
                        EF.Functions.ILike(c.Identification, searchText));
                    break;
            }
        }

        // Limitar a 20 resultados desde la base de datos
        customersQuery = customersQuery.Take(20);

        var customers = await customersQuery.ToListAsync(cancellationToken);

        // Si no hay customers que coincidan con los filtros, retornar lista vacía
        if (customers.Count == 0)
        {
            return [];
        }

        // Segundo: Crear un diccionario de customers filtrados para búsqueda rápida
        var customerLookup = customers.ToDictionary(
            c => (c.DocumentType, c.Identification),
            c => c);

        // Tercero: Consultar todos los Activates y filtrar en memoria
        var activates = await associateDbContext.Activates
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Cuarto: Hacer la intersección en memoria usando foreach
        var result = new List<AssociateDto>();
        var count = 0;

        foreach (var activate in activates)
        {
            if (count >= 20) break;

            var key = (activate.DocumentType, activate.Identification);
            if (customerLookup.TryGetValue(key, out var customer))
            {
                result.Add(new AssociateDto(
                    activate.DocumentType.ToString(),
                    activate.Identification,
                    customer.GetFullName()
                ));
                count++;
            }
        }

        return result;
    }
}