using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

using Security.Application.Contracts.Roles;
using Security.Domain.Roles;

namespace Security.Application.Roles;

public sealed class RoleExistsQueryHandler(
    IRoleRepository roleRepository)
    : IQueryHandler<RoleExistsQuery, bool>
{
    public async Task<Result<bool>> Handle(RoleExistsQuery request, CancellationToken cancellationToken)
    {
        var exists = await roleRepository.ExistsAsync(request.RoleId, cancellationToken);
        return Result.Success(exists);
    }
}
