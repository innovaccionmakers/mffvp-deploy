using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.External;
using Customers.IntegrationEvents.ClientValidation;

namespace Operations.Infrastructure.External.Customers;

internal sealed class PersonValidator(ICapRpcClient rpc) : IPersonValidator
{
    public async Task<Result> ValidateAsync(string idType, string identification, CancellationToken ct)
    {
        var rsp = await rpc.CallAsync<
            ValidatePersonByIdentificationRequest,
            ValidatePersonByIdentificationResponse>(
            nameof(ValidatePersonByIdentificationRequest),
            new ValidatePersonByIdentificationRequest(idType, identification),
            TimeSpan.FromSeconds(5),
            ct);

        return rsp.IsValid
            ? Result.Success()
            : Result.Failure(Error.Validation(rsp.Code, rsp.Message));
    }
}