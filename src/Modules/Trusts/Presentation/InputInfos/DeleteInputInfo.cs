using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.InputInfos.DeleteInputInfo;

namespace Trusts.Presentation.InputInfos;

internal sealed class DeleteInputInfo : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("inputinfos/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteInputInfoCommand(id));
                return result.Match(
                    () => Results.NoContent(),
                    ApiResults.Problem
                );
            })
            .WithTags(Tags.InputInfos);
    }
}