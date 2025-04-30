using MediatR;
using Contributions.Integrations.ClientOperations.UpdateClientOperation;
using Contributions.Integrations.ClientOperations;
using Common.SharedKernel.Presentation.Results;
using Microsoft.AspNetCore.Mvc;
using MFFVP.Api.Application.Contributions;
using Contributions.Integrations.ClientOperations.CreateClientOperation;

namespace MFFVP.Api.BffWeb.Contributions.ClientOperations
{
    public sealed class ClientOperationsEndpoints
    {
        private readonly IClientOperationsService _clientoperationsService;

        public ClientOperationsEndpoints(IClientOperationsService clientoperationsService)
        {
            _clientoperationsService = clientoperationsService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("bffWeb/api/contributions/clientoperations")
                .WithTags("BFF Web - ClientOperations")
                .WithOpenApi();

            group.MapGet("GetAll", async (ISender sender) =>
            {
                var result = await _clientoperationsService.GetClientOperationsAsync(sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .Produces<IReadOnlyCollection<ClientOperationResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapGet("GetById/{id}", async (Guid id, ISender sender) =>
            {
                var result = await _clientoperationsService.GetClientOperationAsync(id, sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .Produces<ClientOperationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPost("Create", async ([FromBody] CreateClientOperationCommand request, ISender sender) =>
            {
                var result = await _clientoperationsService.CreateClientOperationAsync(request, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapPut("Update", async ([FromBody] UpdateClientOperationCommand command, ISender sender) =>
            {
                var result = await _clientoperationsService.UpdateClientOperationAsync(command, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapDelete("Delete/{id}", async (Guid id, ISender sender) =>
            {
                var result = await _clientoperationsService.DeleteClientOperationAsync(id, sender);
                return result.Match(() => Results.NoContent(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}