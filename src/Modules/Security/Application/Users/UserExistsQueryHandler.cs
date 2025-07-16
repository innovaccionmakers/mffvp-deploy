using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using Security.Application.Contracts.Users;
using Security.Domain.Users;

namespace Security.Application.Users;

public sealed class UserExistsQueryHandler(
    IUserRepository userRepository)
    : IQueryHandler<UserExistsQuery, bool>
{
    public async Task<Result<bool>> Handle(UserExistsQuery request, CancellationToken cancellationToken)
    {
        var exists = await userRepository.ExistsAsync(request.UserId, cancellationToken);
        return Result.Success(exists);
    }
}