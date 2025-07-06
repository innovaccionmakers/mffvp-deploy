using Common.SharedKernel.Domain.Auth.Permissions;
using Common.SharedKernel.Presentation.Results;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Security.Application.Contracts.RolePermissions;
using Security.Application.Contracts.UserRoles;

namespace Security.Presentation.MinimalApis;

public static class SecurityBusinessApi
{
    public static void MapSecurityBusinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/v1/FVP/Security")
            .WithTags("Security")
            .WithOpenApi();

        group.MapGet("Permissions", () =>
            {
                Dictionary<string, string> All = MakersPermissionsOperationsAuxiliaryInformations.All
                    .Concat(MakersPermissionsOperationsClientOperations.All)
                    .ToDictionary(p => p.Key, p => p.Value);

                return Results.Ok(All);
            })
            .WithName("Permissions")
            .WithSummary("Retorna una lista de permisos");

        group.MapGet(
            "GetRolePermissions/{roleId:int}",
            async (
                [FromRoute] int roleId,
                ISender sender
            ) =>
            {
                var result = await sender.Send(new GetPermissionsByRoleIdQuery(roleId));
                return result.Value;
            }
            )
            .WithName("GetPermissionsByRoleId")
            .WithSummary("Obtener los permisos asignados a un rol")
            .WithDescription("""
                             Devuelve una lista de permisos asociados rol proporcionado.
                     
                             **Ejemplo de ruta:**
                             `GET /api/v1/FVP/Security/GetRolePermissions/1`
                             """)
            .Produces<IReadOnlyCollection<string>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);


        group.MapPost(
                "CreateRolePermission",
                async (
                    [FromBody] CreateRolePermissionCommand request,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(request);
                    return result.ToApiResult();
                }
            )
            .WithName("CreateRolePermission")
            .WithSummary("Asignar permiso a un rol")
            .WithDescription("""
                             **Ejemplo de petición (application/json):**
                             ```json
                             {
                               "roleId": 1,
                               "scopePermission": "fvp:people:people:view"
                             }
                             ```
                             """)
            .Accepts<CreateRolePermissionCommand>("application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete(
                "DeleteRolePermission/{rolePermissionId:int}",
                async (
                    [FromRoute] int rolePermissionId,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(new DeleteRolePermissionCommand(rolePermissionId));
                    return result.ToApiResult();
                }
            )
            .WithName("DeleteRolePermission")
            .WithSummary("Eliminar un permiso de rol")
            .WithDescription("""
                             Elimina un permiso asignado a un rol específico por su ID.
                             """)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost(
                "UpdateUserRoles",
                async (
                    [FromBody] UpdateUserRolesCommand request,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(request);
                    return result.ToApiResult();
                }
            )
            .WithName("UpdateUserRoles")
            .WithSummary("Actualizar los roles asignados a un usuario")
            .WithDescription("""
                             Sincroniza la lista de RolePermissionIds de un usuario.
                             
                             **Ejemplo de petición (application/json):**
                             ```json
                             {
                               "userId": 1,
                               "rolePermissionsIds": [2, 4, 7]
                             }
                             ```
                             """)
            .Accepts<UpdateUserRolesCommand>("application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
