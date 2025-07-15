using Common.SharedKernel.Presentation.Results;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Security.Application.Contracts.Permissions;
using Security.Application.Contracts.RolePermissions;
using Security.Application.Contracts.UserRoles;
using Security.Domain.RolePermissions;
using Security.Domain.UserRoles;

namespace Security.Presentation.MinimalApis;

public static class SecurityBusinessApi
{
    public static void MapSecurityBusinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/v1/FVP/Security")
            .WithTags("Security")
            .WithOpenApi();

        group.MapGet("Permissions", async (
            ISender sender
            ) =>
                {
                    var result = await sender.Send(new GetAllPermissionsQuery());
                    if (result.IsFailure)
                    {
                        return Results.Problem(
                            title: result.Error.Code,
                            detail: result.Error.Description,
                            statusCode: result.Error.Code switch
                            {
                                "Permissions.NotFound" => StatusCodes.Status404NotFound,
                                _ => StatusCodes.Status500InternalServerError
                            }
                        );
                    }

                    var simplified = result.Value
                        .Select(p => new PermissionDto
                        {
                            ScopePermission = p.ScopePermission,
                            DisplayName = p.DisplayName,
                            Description = p.Description
                        })
                        .ToList();

                    return Results.Ok(simplified);
                })
            .WithName("Permissions")
            .WithSummary("Retorna una lista de permisos")
            .WithDescription("""
                             Retorna solo los permisos disponibles con sus identificadores y descripciones.
                 
                             **Ejemplo de respuesta (application/json):**
                             ```json
                             [
                               {
                                 "scopePermission": "fvp:associate:activates:view",
                                 "displayName": "FVP:Asociados:Activaciones:Ver",
                                 "description": "Permite ver activaciones de asociados."
                               }
                             ]
                             ```
                             """)
            .Produces<List<PermissionDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet(
            "GetRolePermissions/{roleId:int}",
            async (
                [FromRoute] int roleId,
                ISender sender
            ) =>
            {
                var result = await sender.Send(new GetPermissionsByRoleIdQuery(roleId));

                if (result.IsFailure)
                {
                    return Results.Problem(
                        title: result.Error.Code,
                        detail: result.Error.Description,
                        statusCode: result.Error.Code switch
                        {
                            "Role.NotFound" => StatusCodes.Status404NotFound,
                            _ => StatusCodes.Status500InternalServerError
                        }
                    );
                }

                return Results.Ok(result.Value);
            }
            )
            .WithName("GetPermissionsByRoleId")
            .WithSummary("Obtener los permisos asignados a un rol")
            .WithDescription("""
                             Devuelve una lista de permisos asociados rol proporcionado.
                     
                             **Ejemplo de ruta:**
                             `GET /api/v1/FVP/Security/GetRolePermissions/1`
                             """)
            .Produces<IReadOnlyCollection<RolePermissionDto>>(StatusCodes.Status200OK)
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

                    if (result.IsFailure)
                    {
                        return Results.Problem(
                            title: result.Error.Code,
                            detail: result.Error.Description,
                            statusCode: result.Error.Code switch
                            {
                                "Role.NotFound" => StatusCodes.Status404NotFound,
                                "RolePermission.Exists" => StatusCodes.Status409Conflict,
                                "Permission.Required" => StatusCodes.Status400BadRequest,
                                _ => StatusCodes.Status500InternalServerError
                            }
                        );
                    }

                    return Results.Ok(result.Value);
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
            .Produces<int>(StatusCodes.Status200OK)
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

        group.MapGet(
                "GetUserRoles/{userId:int}",
                async (
                    [FromRoute] int userId,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(new GetUserRolesQuery(userId));

                    if (result.IsFailure)
                    {
                        return Results.Problem(
                            title: result.Error.Code,
                            detail: result.Error.Description,
                            statusCode: result.Error.Code switch
                            {
                                "User.NotFound" => StatusCodes.Status404NotFound,
                                _ => StatusCodes.Status500InternalServerError
                            }
                        );
                    }

                    return Results.Ok(result.Value);
                }
            )
            .WithName("GetUserRoles")
            .WithSummary("Obtener los roles asignados a un usuario")
            .WithDescription("""
                             Devuelve la lista de roles asignados a un usuario incluyendo el nombre del rol.
                             
                             **Ejemplo de respuesta (application/json):**
                             ```json
                             [
                               {
                                 "roleId": 2,
                                 "roleName": "Administrador"
                               },
                               {
                                 "roleId": 4,
                                 "roleName": "Auditor"
                               }
                             ]
                             ```
                             """)
            .Produces<IReadOnlyCollection<UserRoleDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
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
                               "roleIds": [2, 4, 7]
                             }
                             ```
                             """)
            .Accepts<UpdateUserRolesCommand>("application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
