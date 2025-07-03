using MediatR;
using Customers.Integrations.People;
using Common.SharedKernel.Presentation.Results;
using Microsoft.AspNetCore.Mvc;
using MFFVP.Api.Application.Customers;
using Common.SharedKernel.Presentation.Filters;
using Integrations.People.CreatePerson;

namespace MFFVP.Api.BffWeb.Customers.Customers;

public sealed class CustomersEndpoints
{
    private readonly ICustomersService _customersService;

    public CustomersEndpoints(ICustomersService customersService)
    {
        _customersService = customersService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        //var group = app.MapGroup("FVP/Customer")
        //    .WithTags("Customers")
        //    .WithOpenApi();

        //group.MapGet("GetCustomer", async (ISender sender) =>
        //{
        //    var result = await _customersService.GetPersonsAsync(sender);
        //    return result.Value;
        //})
        //.WithSummary("Retorna una lista de clientes")
        //.Produces<IReadOnlyCollection<PersonResponse>>(StatusCodes.Status200OK)
        //.ProducesProblem(StatusCodes.Status500InternalServerError);

        //group.MapPost("PostCustomer", async ([FromBody] CreatePersonRequestCommand request, ISender sender) =>
        //{
        //    var result = await _customersService.CreatePersonAsync(request, sender);
        //    return result.ToApiResult(result.Description);
        //})
        //.WithSummary("Crea un cliente")
        //.WithDescription("""
        //                     **Ejemplo de petici�n (application/json):**
        //                     ```json
        //                     {
        //                       "CodigoHomologado": "string",
        //                       "TipoIdentificacion": "C",
        //                       "Identificacion": "12345689",
        //                       "PrimerNombre": "Primera",
        //                       "SegundoNombre": "",
        //                       "PrimerApellido": "Prueba",
        //                       "SegundoApellido": "",
        //                       "FechaNacimiento": "2025-06-13T17:18:12.576Z",
        //                       "Celular": "987654321",
        //                       "Sexo": "M",
        //                       "Direccion": "Calle",
        //                       "Departamento": "91",
        //                       "Municipio": "5002",
        //                       "PaisResidencia": "1",
        //                       "Email": "priemera@prueba.com",
        //                       "ActividadEconomica": "10",
        //                       "Declarante": true,
        //                       "PerfilRiesgo": "MOD",
        //                       "TipoInversionista": "INV"
        //                     }
        //                     ```
        //                     """)
        //.AddEndpointFilter<TechnicalValidationFilter<CreatePersonRequestCommand>>()
        //.Produces<PersonResponse>()
        //.ProducesProblem(StatusCodes.Status400BadRequest)
        //.ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
