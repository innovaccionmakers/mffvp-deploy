using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Products.Integrations.EntityValidation;

namespace Products.IntegrationEvents.EntityValidation;

public sealed class ValidateEntityConsumer : IRpcHandler<ValidateEntityRequest, ValidateEntityResponse>
{
    private readonly ISender mediator;

    public ValidateEntityConsumer(ISender mediator)
    {
        this.mediator = mediator;
    }

    public async Task<ValidateEntityResponse> HandleAsync(ValidateEntityRequest message, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ValidateEntityQuery(message.Entity),
            cancellationToken);

        return result.Match(
            _ => new ValidateEntityResponse(true, null, null),
            err => new ValidateEntityResponse(false, err.Error.Code, err.Error.Description));
    }
}
