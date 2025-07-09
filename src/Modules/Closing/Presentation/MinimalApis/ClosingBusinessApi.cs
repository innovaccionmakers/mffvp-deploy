using Closing.Integrations.ProfitLosses.GetProfitandLoss;
using Closing.Integrations.ProfitLosses.ProfitandLossLoad;
using Closing.Presentation.MinimalApis.PreClosing;
using Closing.Presentation.MinimalApis.ClosingWorkflow;
using Common.SharedKernel.Presentation.Results;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Closing.Presentation.MinimalApis;

public static class ClosingBusinessApi
{
    public static void MapClosingBusinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/v1/FVP/closing/profit-losses")
                .WithTags("Profit & Loss")
                .WithOpenApi();

        group.MapPost(
                    "LoadProfitLoss",
                    async (
                        [Microsoft.AspNetCore.Mvc.FromBody] ProfitandLossLoadCommand request,
                        ISender sender
                    ) =>
                    {
                        var result = await sender.Send(request);
                        return result.ToApiResult();
                    }
                )
                .WithName("LoadProfitLoss")
                .WithSummary("Carga datos de Profit & Loss para un portafolio")
                .WithDescription("""
                                 **Ejemplo de llamada:**

                                 ```http
                                 POST /FVP/closing/profit-losses/LoadProfitLoss
                                 Content-Type: application/json

                                 {
                                   "portfolioId": 123,
                                   "effectiveDate": "2024-01-15T00:00:00Z",
                                   "conceptAmounts": {
                                     "Rendimientos Brutos": 1000.50,
                                     "Gastos": 500.25
                                   }
                                 }
                                 ```

                                 - `portfolioId`: Identificador del portafolio
                                 - `effectiveDate`: Fecha efectiva de los datos
                                 - `conceptAmounts`: Diccionario con nombre del concepto y su monto
                                 """)
                .WithOpenApi(operation =>
                {
                    operation.RequestBody.Description = "Datos para cargar Profit & Loss";
                    return operation;
                })
                .Produces<bool>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet(
                    "GetProfitandLoss",
                    async (
                        [FromQuery] int portfolioId,
                        [FromQuery] DateTime effectiveDate,
                        ISender sender
                    ) =>
                    {
                        var request = new GetProfitandLossQuery(portfolioId, effectiveDate);
                        var result = await sender.Send(request);
                        return result.ToApiResult();
                    }
                )
                .WithName("GetProfitandLoss")
                .WithSummary("Consulta de PyG - Obtiene valores de Profit & Loss para un portafolio y fecha específicos")
                .WithDescription("""
                                 **Ejemplo de llamada:**

                                 ```http
                                 GET /FVP/closing/profit-losses/GetProfitandLoss?portfolioId=123&effectiveDate=2024-01-15T00:00:00Z
                                 ```

                                 **Respuesta:**
                                 ```json
                                 {
                                   "values": {
                                     "Rendimientos Brutos": 1000.50,
                                     "Gastos Administrativos": 500.25,
                                     "Comisiones": 250.00
                                   },
                                   "netYield": 250.25
                                 }
                                 ```

                                 - `portfolioId`: Identificador del portafolio
                                 - `effectiveDate`: Fecha efectiva para la consulta
                                 - `values`: Estructura llave-valor donde la llave es el nombre del concepto y el valor es el monto
                                 - `netYield`: Rendimientos Netos (suma de ingresos menos suma de gastos)
                                 """)
                .WithOpenApi(operation =>
                {
                    operation.Parameters[0].Description = "Identificador del portafolio";
                    operation.Parameters[1].Description = "Fecha efectiva para la consulta";
                    return operation;
                })
                .Produces<GetProfitandLossResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

        PreClosingEndpoints.MapPreclosingEndpoints(app);
        ClosingWorkflowEndpoints.MapClosingWorkflowEndpoints(app);
    }
}
