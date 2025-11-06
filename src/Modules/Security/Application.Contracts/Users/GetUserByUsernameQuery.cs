using Common.SharedKernel.Application.Messaging;
using Security.Domain.Users;

namespace Security.Application.Contracts.Users;

public sealed record GetUserByUsernameQuery(string Username) : IQuery<User?>;
