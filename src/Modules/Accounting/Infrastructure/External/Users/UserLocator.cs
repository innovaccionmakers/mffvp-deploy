using Accounting.Application.Abstractions.External;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using Security.IntegrationEvents.Users.GetByUsername;
using Common.SharedKernel.Core.Primitives;

namespace Accounting.Infrastructure.External.Users;

public class UserLocator(IRpcClient rpc) : IUserLocator
{
    public async Task<Result<string?>> GetEmailUserAsync(string userName, CancellationToken ct)
    {
        var rc = await rpc.CallAsync<
            GetUserByUsernameRequest,
            GetUserByUsernameResponse>(new GetUserByUsernameRequest(userName), ct);

        return rc.Succeeded
            ? Result.Success<string?>(rc.User!.Email)
            : Result.Failure<string?>(Error.Validation(rc.Code!, rc.Message!));
    }
}
