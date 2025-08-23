using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Security.Application.Abstractions.Data;
using Security.Application.Contracts.Roles;
using Security.Domain.Roles;

namespace Security.Application.Roles;

public sealed class CreateRoleCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRoleCommand, int>
{
    public async Task<Result<int>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
        {
            return Result.Failure<int>(Error.Validation(
                "Role.Id.Invalid",
                "The role ID must be greater than zero."));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure<int>(Error.Validation(
                "Role.Name.Required",
                "The role name is required."));
        }

        var exists = await roleRepository.GetAsync(request.Id, cancellationToken);
        if (exists is not null)
        {
            return Result.Failure<int>(Error.Conflict(
                "Role.Exists",
                "A role with the specified ID already exists."));
        }

        var result = Role.Create(request.Id, request.Name, request.Objective);

        if (result.IsFailure)
            return Result.Failure<int>(result.Error);

        var role = result.Value;

        roleRepository.Insert(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(role.Id);
    }

}