using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Portfolios.UpdatePortfolio;

namespace Products.Presentation.Portfolios;

internal sealed class UpdatePortfolio : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("portfolios/{id:long}", async (long id, Request request, ISender sender) =>
            {
                var command = new UpdatePortfolioCommand(
                    id,
                    request.NewStandardCode,
                    request.NewName,
                    request.NewShortName,
                    request.NewModalityId,
                    request.NewInitialMinimumAmount
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Portfolios);
    }

    internal sealed class Request
    {
        public string NewStandardCode { get; set; }
        public string NewName { get; set; }
        public string NewShortName { get; set; }
        public int NewModalityId { get; set; }
        public decimal NewInitialMinimumAmount { get; set; }
    }
}