using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.IntegrationEvents.DocumentTypeValidation;
using Products.Application.Abstractions.Services.External;

namespace Products.Infrastructure.External.DocumentTypes;

internal sealed class DocumentTypeValidator(ICapRpcClient rpc) : IDocumentTypeValidator
{
    public async Task<Result> EnsureExistsAsync(string code, CancellationToken ct)
    {
        var reply = await rpc.CallAsync<
            GetDocumentTypeIdByCodeRequest,
            GetDocumentTypeIdByCodeResponse>(
            nameof(GetDocumentTypeIdByCodeRequest),
            new(code),
            TimeSpan.FromSeconds(5),
            ct);

        return reply.Succeeded
            ? Result.Success()
            : Result.Failure(Error.Validation(reply.Code, reply.Message));
    }
}
