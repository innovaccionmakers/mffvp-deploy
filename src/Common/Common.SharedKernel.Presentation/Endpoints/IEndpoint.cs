using Microsoft.AspNetCore.Routing;

namespace Common.SharedKernel.Presentation.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}

