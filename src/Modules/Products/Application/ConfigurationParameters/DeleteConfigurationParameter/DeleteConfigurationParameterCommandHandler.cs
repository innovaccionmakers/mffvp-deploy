using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.ConfigurationParameters;
using Products.Integrations.ConfigurationParameters.DeleteConfigurationParameter;
using Products.Application.Abstractions.Data;

namespace Products.Application.ConfigurationParameters.DeleteConfigurationParameter
{
    internal sealed class DeleteConfigurationParameterCommandHandler(
        IConfigurationParameterRepository repository,
        IUnitOfWork unitOfWork
    ) : ICommandHandler<DeleteConfigurationParameterCommand>
    {
        public async Task<Result> Handle(
            DeleteConfigurationParameterCommand request,
            CancellationToken cancellationToken)
        {
            await using DbTransaction tx = await unitOfWork.BeginTransactionAsync(cancellationToken);

            var parameter = await repository.GetAsync(request.ConfigurationParameterId, cancellationToken);
            if (parameter is null)
                return Result.Failure(ConfigurationParameterErrors.NotFound(request.ConfigurationParameterId));

            repository.Delete(parameter);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}