using Common.SharedKernel.Application.Messaging;

using Security.Domain.Users;

namespace Security.Application.Contracts.Users;

public sealed class RefreshTokenQuery : IQuery<UserForAuthenticationDto>
{
}
