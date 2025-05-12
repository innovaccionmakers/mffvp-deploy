using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.InputInfos.GetInputInfos;

namespace Trusts.Presentation.InputInfos;

internal sealed class GetInputInfos : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("inputinfos", async (ISender sender) =>
            {
                var result = await sender.Send(new GetInputInfosQuery());
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.InputInfos);
    }
}