using Accounting.Application.Abstractions.External;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Products.IntegrationEvents.Administrators.GetFirstAdministrator;
using Products.Integrations.Administrators;

namespace Accounting.Infrastructure.External.Administrators;

public sealed class AdministratorLocator(IRpcClient rpc) : IAdministratorLocator
{
    public async Task<Result<AdministratorResponse?>> GetFirstAdministratorAsync(CancellationToken cancellationToken = default)
    {
        var response = await rpc.CallAsync<GetFirstAdministratorRequest, GetFirstAdministratorResponse>(
            new GetFirstAdministratorRequest(),
            cancellationToken);

        return response.Succeeded
            ? Result.Success(response.Administrator)
            : Result.Failure<AdministratorResponse?>(Error.Validation(response.Code!, response.Message!));
    }
}

