using Common.SharedKernel.Domain;
using Common.SharedKernel.Presentation.Results;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Security.Application.Contracts.Roles;

namespace Security.Presentation.MinimalApis;

public static class SecurityBusinessApi
{
    public static void MapSecurityBusinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/v1/FVP/Security")
            .WithTags("Security")
            .WithOpenApi();

        group.MapPost(
                "AssignRolePermission",
                async (
                    [FromBody] AssignRolePermissionCommand request,
                    ISender sender
                ) =>
                {
                    var result = new Result(true, new Error("", "", ErrorType.NotFound)); // await sender.Send(request);
                    return result.ToApiResult();
                }
            )
            .WithName("AssignRolePermission")
            .WithSummary("Asignar permisos a un rol")
            .WithDescription("""
                             **Ejemplo de petición (application/json):**
                             ```json
                             {
                               "roleId": 1,
                               "scopePermissions": [
                                 "fvp:domain:users.view",
                                 "fvp:domain:users.create"
                               ]
                             }
                             ```
                             """)
            .Accepts<AssignRolePermissionCommand>("application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
