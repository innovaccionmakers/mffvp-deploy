using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Presentation.Results;
using DotNetCore.CAP;
using MediatR;
using Customers.Integrations.DocumentTypes.GetDocumentTypeId;

namespace Customers.IntegrationEvents.DocumentTypeValidation;

public sealed class DocumentTypeValidationConsumer : ICapSubscribe
{
    private readonly ISender _mediator;

    public DocumentTypeValidationConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    [CapSubscribe(nameof(GetDocumentTypeIdByCodeRequest))]
    public async Task<GetDocumentTypeIdByCodeResponse> GetDocumentTypeIdAsync(
        GetDocumentTypeIdByCodeRequest message,
        [FromCap] CapHeader header,
        CancellationToken cancellationToken)
    {
        var corr = header[CapRpcClient.Headers.CorrelationId];
        header.AddResponseHeader(CapRpcClient.Headers.CorrelationId, corr);

        var result = await _mediator.Send(
            new GetDocumentTypeIdQuery(message.TypeIdHomologationCode),
            cancellationToken);

        return result.Match(
            ok => new GetDocumentTypeIdByCodeResponse(true, ok.DocumentTypeId, null, null),
            err => new GetDocumentTypeIdByCodeResponse(false, null, err.Error.Code, err.Error.Description));
    }
}