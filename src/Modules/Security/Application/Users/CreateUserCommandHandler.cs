using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Security.Application.Abstractions.Data;
using Security.Application.Contracts.Users;
using Security.Domain.Users;

namespace Security.Application.Users;

public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateUserCommand, int>
{
    public async Task<Result<int>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return Result.Failure<int>(Error.Validation(
                "User.UserName.Required",
                "The username is required."));
        }

        var result = User.Create(
            request.Id,
            request.UserName,
            request.Name,
            request.MiddleName,
            request.Identification,
            request.Email,
            request.DisplayName
        );

        if (result.IsFailure)
            return Result.Failure<int>(result.Error);

        var user = result.Value;

        userRepository.Insert(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
