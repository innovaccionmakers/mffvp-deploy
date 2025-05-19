using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Data;
using Products.Domain.ConfigurationParameters;
using Products.Integrations.ConfigurationParameters.DeleteConfigurationParameter;

namespace Products.Application.ConfigurationParameters.DeleteConfigurationParameter;

internal sealed class DeleteConfigurationParameterCommandHandler(
    IConfigurationParameterRepository repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteConfigurationParameterCommand>
{
    public async Task<Result> Handle(
        DeleteConfigurationParameterCommand request,
        CancellationToken cancellationToken)
    {
        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var parameter = await repository.GetAsync(request.ConfigurationParameterId, cancellationToken);
        if (parameter is null)
            return Result.Failure(ConfigurationParameterErrors.NotFound(request.ConfigurationParameterId));

        repository.Delete(parameter);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return Result.Success();
    }
}