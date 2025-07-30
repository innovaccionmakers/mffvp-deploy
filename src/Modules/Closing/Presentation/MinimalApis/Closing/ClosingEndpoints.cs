using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Closing.Presentation.MinimalApis.Closing;

public static class ClosingEndpoints// : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/FVP/closing")
            .WithTags("Closing")
            .WithOpenApi();

        group.MapRunClosingEndpoint();
        group.MapConfirmClosingEndpoint();
        group.MapCancelClosingEndpoint();
    }
}
