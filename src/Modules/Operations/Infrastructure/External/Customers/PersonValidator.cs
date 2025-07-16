using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.External;
using Customers.IntegrationEvents.ClientValidation;

namespace Operations.Infrastructure.External.Customers;

internal sealed class PersonValidator(IRpcClient rpc) : IPersonValidator
{
    public async Task<Result> ValidateAsync(string idType, string identification, CancellationToken ct)
    {
        var rsp = await rpc.CallAsync<
            ValidatePersonByIdentificationRequest,
            ValidatePersonByIdentificationResponse>(
            new ValidatePersonByIdentificationRequest(idType, identification),
            ct);

        return rsp.IsValid
            ? Result.Success()
            : Result.Failure(Error.Validation(rsp.Code, rsp.Message));
    }
}