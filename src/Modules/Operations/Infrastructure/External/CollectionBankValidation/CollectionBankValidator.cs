using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Operations.Application.Abstractions.External;

using Treasury.IntegrationEvents.Issuers.ValidateCollectionBank;

namespace Operations.Infrastructure.External.CollectionBankValidation;

internal sealed class CollectionBankValidator(IRpcClient rpc) : ICollectionBankValidator
{
    public async Task<Result<long>> ValidateAsync(
        string homologatedCode,
        CancellationToken cancellationToken = default)
    {
        var response = await rpc.CallAsync<
            ValidateCollectionBankRequest,
            ValidateCollectionBankResponse>(
            new ValidateCollectionBankRequest(homologatedCode),
            cancellationToken);

        return response.IsValid
            ? Result.Success(response.BankId!.Value)
            : Result.Failure<long>(Error.Validation(response.Code!, response.Message!));
    }
}