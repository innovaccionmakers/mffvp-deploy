using Common.SharedKernel.Application.Messaging;

namespace Security.Application.Contracts.Roles;

public sealed record RoleExistsQuery(int RoleId) : IQuery<bool>;
