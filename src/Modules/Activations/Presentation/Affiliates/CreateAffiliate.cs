using Activations.Integrations.Affiliates.CreateAffiliate;
using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Activations.Presentation.Affiliates;

internal sealed class CreateAffiliate : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("affiliates", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreateAffiliateCommand(
                    request.IdentificationType,
                    request.Identification,
                    request.Pensioner,
                    request.MeetsRequirements,
                    request.ActivationDate
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Affiliates);
    }

    internal sealed class Request
    {
        public string IdentificationType { get; init; }
        public string Identification { get; init; }
        public bool Pensioner { get; init; }
        public bool MeetsRequirements { get; init; }
        public DateTime ActivationDate { get; init; }
    }
}