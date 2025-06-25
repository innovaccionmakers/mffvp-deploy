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
        var customersQuery = customersDbContext.Customers.AsNoTracking();


        if (!string.IsNullOrWhiteSpace(tipoIdentificacion))
        {
            if (Guid.TryParse(tipoIdentificacion, out var documentTypeGuid))
            {
                customersQuery = customersQuery.Where(c => c.DocumentType == documentTypeGuid);
            }
        }

        if (!string.IsNullOrWhiteSpace(searchBy) && !string.IsNullOrWhiteSpace(text))
        {
            var searchText = $"%{text}%";

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

        customersQuery = customersQuery.Take(20);

        var customers = await customersQuery.ToListAsync(cancellationToken);


        if (customers.Count == 0)
        {
            return [];
        }

        
        var customerLookup = customers.ToDictionary(
            c => (c.DocumentType, c.Identification),
            c => c);

        
        var activates = await associateDbContext.Activates
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        
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