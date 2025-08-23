using Common.SharedKernel.Presentation.Results;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Security.Application.Contracts.Permissions;
using Security.Application.Contracts.RolePermissions;
using Security.Application.Contracts.Roles;
using Security.Application.Contracts.UserPermissions;
using Security.Application.Contracts.UserRoles;
using Security.Application.Contracts.Users;

namespace Security.Presentation.MinimalApis;

public static class SecurityBusinessApi
{
    public static void MapSecurityBusinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/v1/FVP/Security")
            .WithTags("Security")
            .WithOpenApi()
            .RequireAuthorization();

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
                            PermissionId = p.PermissionId,
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
            "GetUserPermissions",
            async (
                IHttpContextAccessor accessor,
                ISender sender
            ) =>
            {
                var httpContext = accessor.HttpContext;
                var userName = httpContext?.User?.Identity?.Name;

                if (string.IsNullOrWhiteSpace(userName))
                {
                    return Results.Problem(
                                title: "Unauthorized",
                                detail: "The user identity is not present in the token.",
                                statusCode: StatusCodes.Status401Unauthorized
                            );
                }

                var result = await sender.Send(new GetPermissionsByUserNameQuery(userName));

                if (result.IsFailure)
                {
                    return Results.Problem(
                                title: result.Error.Code,
                                detail: result.Error.Description,
                                statusCode: result.Error.Code switch
                            {
                                "User.UserName.Required" => StatusCodes.Status400BadRequest,
                                "User.NotFound" => StatusCodes.Status404NotFound,
                                _ => StatusCodes.Status500InternalServerError
                            }
                            );
                }

                return Results.Ok(result.Value);
            }
            )
            .WithName("GetUserPermissions")
            .WithSummary("Obtener permisos del usuario autenticado")
            .WithDescription("""
                                        Devuelve la lista de permisos del usuario autenticado, obteniendo el UserName desde el token JWT.

                                        **Ejemplo de respuesta (application/json):**
                                        ```json
                                        [
                                        {
                                            "scopePermission": "fvp:people:people:view",
                                            "displayName": "Ver personas",
                                            "description": "Permite ver la lista de personas"
                                        },
                                        {
                                            "scopePermission": "fvp:people:people:edit",
                                            "displayName": "Editar personas",
                                            "description": "Permite editar registros de personas"
                                        }
                                        ]
                                        ```
                                        """)
            .Produces<IReadOnlyCollection<PermissionDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
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

        group.MapGet(
            "UserExists/{userId:int}",
                async (
                    [FromRoute] int userId,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(new UserExistsQuery(userId));
                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : Results.Problem(result.Error.Description);
                }
            )
            .WithName("UserExists")
            .WithSummary("Verifica si un usuario existe")
            .WithDescription("""
                                     Devuelve `true` o `false` si el usuario con ID especificado existe o no.
                     
                                     **Ejemplo de respuesta:**
                                     ```json
                                     true
                                     ```
                                     """)
            .Produces<bool>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost(
            "CreateUser",
                async (
                    [FromBody] CreateUserCommand request,
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
                                "User.UserName.Required" => StatusCodes.Status400BadRequest,
                                _ => StatusCodes.Status500InternalServerError
                            }
                        );
                    }

                    return Results.Ok(result.Value);
                }
            )
            .WithName("CreateUser")
            .WithSummary("Crear nuevo usuario")
            .WithDescription("""
                             Crea un nuevo usuario con toda la información requerida.

                             **Ejemplo de petición (application/json):**
                             ```json
                             {
                               "id": 42,
                               "userName": "jperez",
                               "name": "Juan",
                               "middleName": "Carlos",
                               "identification": "123456789",
                               "email": "jperez@email.com",
                               "displayName": "Juan Pérez"
                             }
                             ```
                             """)
            .Accepts<CreateUserCommand>("application/json")
            .Produces<int>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet(
                "RoleExists/{roleId:int}",
                async (
                    [FromRoute] int roleId,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(new RoleExistsQuery(roleId));
                    return result.IsSuccess
                        ? Results.Ok(result.Value)
                        : Results.Problem(result.Error.Description);
                }
            )
            .WithName("RoleExists")
            .WithSummary("Verifica si un rol existe")
            .WithDescription("""
                             Devuelve `true` o `false` si el rol con ID especificado existe o no.

                             **Ejemplo de respuesta:**
                             ```json
                             true
                             ```
                             """)
            .Produces<bool>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost(
            "CreateRole",
                async (
                    [FromBody] CreateRoleCommand request,
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
                                "Role.Name.Required" => StatusCodes.Status400BadRequest,
                                _ => StatusCodes.Status500InternalServerError
                            }
                        );
                    }

                    return Results.Ok(result.Value);
                }
            )
            .WithName("CreateRole")
            .WithSummary("Crear nuevo rol")
            .WithDescription("""
                        Crea un nuevo rol con el nombre y objetivo especificados.

                        **Ejemplo de petición (application/json):**
                        ```json
                        {
                        "id": 1,
                        "name": "Administrador",
                        "objective": "Gestión total del sistema"
                        }
                        ```
                        """)
            .Accepts<CreateRoleCommand>("application/json")
            .Produces<int>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet(
            "GetToken",
            async (ISender sender) =>
            {
                var result = await sender.Send(new RefreshTokenQuery());

                if (result.IsFailure)
                {
                    return Results.Problem(
                        title: result.Error.Code,
                        detail: result.Error.Description,
                        statusCode: result.Error.Code switch
                        {
                            "Auth.Token.Missing" or "Auth.Token.Invalid" or "Auth.User.Invalid" => StatusCodes.Status401Unauthorized,
                            "Auth.User.NotFound" => StatusCodes.Status404NotFound,
                            _ => StatusCodes.Status500InternalServerError
                        });
                }

                return Results.Ok(result.Value);
            })
            .WithName("GetToken")
            .WithSummary("Renueva el token JWT usando el token actual en la cookie")
            .WithDescription("""
                             Este endpoint verifica el token JWT actual enviado como cookie (`authToken`)
                             y devuelve uno nuevo si es válido.

                             **Requiere:** la cookie `authToken` enviada por el cliente.

                             **Ejemplo de respuesta:**
                             ```json
                             "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
                             ```
                             """)
            .Produces<string>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

    }
}
