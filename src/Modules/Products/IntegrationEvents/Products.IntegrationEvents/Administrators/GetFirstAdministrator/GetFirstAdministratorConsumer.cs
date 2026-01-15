using Common.SharedKernel.Application.Rpc;
using MediatR;
using Products.Integrations.Administrators.GetFirstAdministrator;

namespace Products.IntegrationEvents.Administrators.GetFirstAdministrator;

public sealed class GetFirstAdministratorConsumer(ISender sender)
    : IRpcHandler<GetFirstAdministratorRequest, GetFirstAdministratorResponse>
{
    public async Task<GetFirstAdministratorResponse> HandleAsync(
        GetFirstAdministratorRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetFirstAdministratorQuery(), ct);

        if (!result.IsSuccess)
        {
            return new GetFirstAdministratorResponse(
                false,
                null,
                result.Error.Code,
                result.Error.Description);
        }

        return new GetFirstAdministratorResponse(
            true,
            result.Value,
            null,
            null);
    }
}

