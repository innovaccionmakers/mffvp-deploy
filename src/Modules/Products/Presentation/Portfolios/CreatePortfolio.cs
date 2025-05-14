using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Portfolios.CreatePortfolio;

namespace Products.Presentation.Portfolios
{
    internal sealed class CreatePortfolio : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("portfolios", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreatePortfolioCommand(
                    request.StandardCode, 
                    request.Name, 
                    request.ShortName, 
                    request.ModalityId, 
                    request.InitialMinimumAmount
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Portfolios);
        }

        internal sealed class Request
        {
            public string StandardCode { get; init; }
            public string Name { get; init; }
            public string ShortName { get; init; }
            public int ModalityId { get; init; }
            public decimal InitialMinimumAmount { get; init; }
        }
    }
}