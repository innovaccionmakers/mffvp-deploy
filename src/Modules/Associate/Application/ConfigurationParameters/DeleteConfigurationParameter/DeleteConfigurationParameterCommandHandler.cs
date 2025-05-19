using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Associate.Domain.ConfigurationParameters;
using Associate.Integrations.ConfigurationParameters.DeleteConfigurationParameter;
using Associate.Application.Abstractions.Data;

namespace Associate.Application.ConfigurationParameters.DeleteConfigurationParameter;

internal sealed class DeleteConfigurationParameterCommandHandler(
    IConfigurationParameterRepository configurationparameterRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteConfigurationParameterCommand>
{
    public async Task<Result> Handle(DeleteConfigurationParameterCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var configurationparameter = await configurationparameterRepository.GetAsync(request.ConfigurationParameterId, cancellationToken);
        if (configurationparameter is null)
        {
            return Result.Failure(ConfigurationParameterErrors.NotFound(request.ConfigurationParameterId));
        }
        
        configurationparameterRepository.Delete(configurationparameter);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}