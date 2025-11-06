using Common.SharedKernel.Application.Rpc;
using MediatR;
using Security.Application.Contracts.Users;

namespace Security.IntegrationEvents.Users.GetByUsername;

public class GetUserByUsernameConsumer(ISender sender) : IRpcHandler<GetUserByUsernameRequest, GetUserByUsernameResponse>
{
    public async Task<GetUserByUsernameResponse> HandleAsync(GetUserByUsernameRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new GetUserByUsernameQuery(request.Username), ct);

        if (!result.IsSuccess)
            return new GetUserByUsernameResponse(false, null, result.Error.Code, result.Error.Description);

        if (result?.Value is null)
            return new GetUserByUsernameResponse(false, null, "User.NotFound", "Usuario no encontrado");

        return new GetUserByUsernameResponse(true, new UserResponse(result.Value.Id, result.Value.UserName, result.Value.Name, result.Value.MiddleName, result.Value.Identification, result.Value.Email, result.Value.DisplayName), null, null);
    }
}
