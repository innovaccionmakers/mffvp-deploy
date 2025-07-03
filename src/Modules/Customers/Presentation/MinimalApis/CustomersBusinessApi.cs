using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;

using Customers.Integrations.People;
using Customers.Integrations.People.GetPersons;

using Integrations.People.CreatePerson;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Customers.Presentation.MinimalApis;

public static class CustomersBusinessApi
{
    public static void MapCustomersBusinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/v1/FVP/Customer")
            .WithTags("Customers")
            .WithOpenApi();

        group.MapGet("GetCustomer", async (ISender sender) =>
        {
            var result = await sender.Send(new GetPersonsQuery());
            return result.Value;
        })
        .WithSummary("Retorna una lista de clientes")
        .Produces<IReadOnlyCollection<PersonResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("PostCustomer", async ([Microsoft.AspNetCore.Mvc.FromBody] CreatePersonRequestCommand request, ISender sender) =>
        {
            var result = await sender.Send(request);
            return result.ToApiResult(result.Description);
        })
        .WithSummary("Crea un cliente")
        .WithDescription("""
                             **Ejemplo de petici�n (application/json):**
                             ```json
                             {
                               "CodigoHomologado": "string",
                               "TipoIdentificacion": "C",
                               "Identificacion": "12345689",
                               "PrimerNombre": "Primera",
                               "SegundoNombre": "",
                               "PrimerApellido": "Prueba",
                               "SegundoApellido": "",
                               "FechaNacimiento": "2025-06-13T17:18:12.576Z",
                               "Celular": "987654321",
                               "Sexo": "M",
                               "Direccion": "Calle",
                               "Departamento": "91",
                               "Municipio": "5002",
                               "PaisResidencia": "1",
                               "Email": "priemera@prueba.com",
                               "ActividadEconomica": "10",
                               "Declarante": true,
                               "PerfilRiesgo": "MOD",
                               "TipoInversionista": "INV"
                             }
                             ```
                             """)
        .AddEndpointFilter<TechnicalValidationFilter<CreatePersonRequestCommand>>()
        .Produces<PersonResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
