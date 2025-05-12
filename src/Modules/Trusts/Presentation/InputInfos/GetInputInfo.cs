using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.InputInfos.GetInputInfo;

namespace Trusts.Presentation.InputInfos;

internal sealed class GetInputInfo : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("inputinfos/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetInputInfoQuery(id));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.InputInfos);
    }
}