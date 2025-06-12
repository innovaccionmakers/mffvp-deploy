using Common.SharedKernel.Presentation.Filters;
using MediatR;
using Products.Integrations.Portfolios;
using Common.SharedKernel.Presentation.Results;
using MFFVP.Api.Application.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;

namespace MFFVP.Api.BffWeb.Products.Portfolios
{
    public sealed class PortfoliosEndpoints
    {
        private readonly IPortfoliosService _portfoliosService;

        public PortfoliosEndpoints(IPortfoliosService portfoliosService)
        {
            _portfoliosService = portfoliosService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("bffWeb/FVP/products/portfolios")
                .WithTags("BFF Web - Portfolios")
                .WithOpenApi();

            group.MapGet(
                    "GetById",
                    async (
                        [FromQuery] int portfolioId,
                        ISender sender
                    ) =>
                    {
                        var result = await _portfoliosService
                            .GetPortfolioAsync(portfolioId, sender);
                        return result.ToApiResult();
                    }
                )
                .WithName("GetPortfolioById")
                .WithSummary("Obtiene un portafolio por su identificador")
                .WithDescription("""
                                 **Ejemplo de llamada:**

                                 ```http
                                 GET /bffWeb/FVP/products/portfolios/GetById?portfolioId=123
                                 ```

                                 - `portfolioId`: Identificador del portafolio (e.g., 123)
                                 """)
                .WithOpenApi(operation =>
                {
                    var p = operation.Parameters.First(p => p.Name == "portfolioId");
                    p.Description = "Identificador único del portafolio";
                    p.Example     = new OpenApiInteger(123);
                    return operation;
                })
                .Produces<PortfolioResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status500InternalServerError);
            
            group.MapGet(
                    "GetAllPortfolios",
                    async (ISender sender) =>
                    {
                        var result = await _portfoliosService.GetPortfoliosAsync(sender);
                        return result.ToApiResult();
                    }
                )
                .WithName("GetAllPortfolios")
                .WithSummary("Obtiene todos los portafolios")
                .WithDescription("""
                                 **Ejemplo de llamada:**

                                 ```http
                                 GET /bffWeb/FVP/products/portfolios/GetAllPortfolios
                                 ```
                                 """)
                .Produces<IReadOnlyCollection<PortfolioResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}