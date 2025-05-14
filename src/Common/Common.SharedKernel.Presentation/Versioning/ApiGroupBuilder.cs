using Asp.Versioning.Conventions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Common.SharedKernel.Presentation.Versioning;

public static class ApiGroupBuilder
{
    public static RouteGroupBuilder CreateVersionedApiGroup(this IEndpointRouteBuilder app)
    {
        var versions = app.NewApiVersionSet()
            .HasApiVersion(1, 0)
            .HasApiVersion(2, 0)
            .ReportApiVersions()
            .Build();

        return app.MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(versions);
    }
}
