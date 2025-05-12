using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Activations.Integrations.Affiliates.UpdateAffiliate;

namespace Activations.Presentation.Affiliates
{
    internal sealed class UpdateAffiliate : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("affiliates/{id:int}", async (int id, Request request, ISender sender) =>
            {
                var command = new UpdateAffiliateCommand(
                    id,
                    request.NewIdentificationType, 
                    request.NewIdentification, 
                    request.NewPensioner, 
                    request.NewMeetsRequirements, 
                    request.NewActivationDate
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Affiliates);
        }

        internal sealed class Request
        {
            public string NewIdentificationType { get; set; }
            public string NewIdentification { get; set; }
            public bool NewPensioner { get; set; }
            public bool NewMeetsRequirements { get; set; }
            public DateTime NewActivationDate { get; set; }
        }
    }
}