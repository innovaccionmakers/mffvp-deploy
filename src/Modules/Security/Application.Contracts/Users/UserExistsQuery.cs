using Common.SharedKernel.Application.Messaging;

namespace Security.Application.Contracts.Users;

public sealed record UserExistsQuery(int UserId) : IQuery<bool>;
