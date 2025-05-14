using Associate.Integrations.Activates.CreateActivate;
using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Associate.Presentation.Activates;

internal sealed class CreateActivate : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("activates", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreateActivateResponseCommand(
                    request.IdentificationType,
                    request.Identification,
                    request.Pensioner,
                    request.MeetsRequirements,
                    request.ActivateDate
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Activates);
    }

    internal sealed class Request
    {
        public string IdentificationType { get; init; }
        public string Identification { get; init; }
        public bool Pensioner { get; init; }
        public bool MeetsRequirements { get; init; }
        public DateTime ActivateDate { get; init; }
    }
}