using Common.SharedKernel.Domain;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

using Operations.Integrations.Contributions.CreateContribution;
using Operations.Integrations.SubTransactionTypes;

namespace Operations.Presentation.MinimalApis;

public static class OperationsBusinessApi
{
    public static void MapOperationsBusinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/v1/FVP/Operations")
            .WithTags("Operations")
            .WithOpenApi();

        group.MapPost(
                "ContributionTx",
                [Authorize(Policy = "fvp:operations:contributiontx:create")]
                async (
                    [Microsoft.AspNetCore.Mvc.FromBody] CreateContributionCommand request,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(request);
                    return result.ToApiResult();
                }
            )
            .WithName("CreateContribution")
            .WithSummary("Registrar un aporte")
            .WithDescription("""
                             **Ejemplo de petición (application/json):**
                             ```json
                             {
                               "TipoId": "C",
                               "Identificacion": "123456789",
                               "IdObjetivo": 456,
                               "IdPortafolio": "ALT123",
                               "Valor": 1500.75,
                               "Origen": "Sucursal",
                               "ModalidadOrigen": "Efectivo",
                               "MetodoRecaudo": "POS",
                               "FormaPago": "Tarjeta",
                               "DetalleFormaPago": { "cardNumber": "**** **** **** 1234", "expiry": "12/25" },
                               "BancoRecaudo": "Banco X",
                               "CuentaRecaudo": "000123456",
                               "AporteCertificado": "CERT123",
                               "RetencionContingente": 50.25,
                               "FechaConsignacion": "2025-06-01T00:00:00Z",
                               "FechaEjecucion": "2025-06-02T00:00:00Z",
                               "UsuarioComercial": "user123",
                               "MedioVerificable": { "url": "http://example.com/recibo.pdf" },
                               "Subtipo": "Extra",
                               "Canal": "Online",
                               "Usuario": "system"
                             }
                             ```
                             """)
            .AddEndpointFilter<TechnicalValidationFilter<CreateContributionCommand>>()
            .Accepts<CreateContributionCommand>("application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        
        group.MapGet(
                "OperationTypes",
                async (ISender sender) =>
                {
                    var result = await sender.Send(new GetAllOperationTypesQuery());
                    return result.Match(
                        Results.Ok,
                        r => r.Error.Type == ErrorType.Validation
                            ? ApiResults.Failure(r)
                            : ApiResults.Problem(r)
                    );
                }
            )
            .WithName("GetAllOperationTypes")
            .WithSummary("Obtiene la lista completa de tipos de operación")
            .Produces<IReadOnlyCollection<SubtransactionTypeResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        
    }
}
