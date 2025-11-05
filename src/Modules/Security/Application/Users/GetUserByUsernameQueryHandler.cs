using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Security.Application.Contracts.Users;
using Security.Domain.Users;

namespace Security.Application.Users;

public class GetUserByUsernameQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUserByUsernameQuery, User?>
{
    public async Task<Result<User?>> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByUserNameAsync(request.Username, cancellationToken);
        return Result.Success(user);
    }
}
