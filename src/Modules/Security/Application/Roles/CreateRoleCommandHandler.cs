using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using Security.Application.Abstractions.Data;
using Security.Application.Contracts.Roles;
using Security.Domain.Roles;

using System.Data.Common;

namespace Security.Application.Roles;

public sealed class CreateRoleCommandHandler(
    IRoleRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRoleCommand>
{
    public async Task<Result> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
        {
            return Result.Failure(Error.Validation(
                "Role.Id.Invalid",
                "The role ID must be greater than zero."));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure(Error.Validation(
                "Role.Name.Required",
                "The role name is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Objective))
        {
            return Result.Failure(Error.Validation(
                "Role.Objective.Required",
                "The role objective is required."));
        }

        var exists = await repository.GetAsync(request.Id, cancellationToken);
        if (exists is not null)
        {
            return Result.Failure(Error.Conflict(
                "Role.Exists",
                "A role with the specified ID already exists."));
        }

        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var role = Role.Create(request.Id, request.Name, request.Objective);

        repository.Insert(role.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}