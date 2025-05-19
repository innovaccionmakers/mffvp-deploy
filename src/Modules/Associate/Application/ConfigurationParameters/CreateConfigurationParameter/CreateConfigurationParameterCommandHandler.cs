using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Associate.Domain.ConfigurationParameters;
using Associate.Integrations.ConfigurationParameters.CreateConfigurationParameter;
using Associate.Integrations.ConfigurationParameters;
using Associate.Application.Abstractions.Data;

namespace Associate.Application.ConfigurationParameters.CreateConfigurationParameter

{
    internal sealed class CreateConfigurationParameterCommandHandler(
        IConfigurationParameterRepository configurationparameterRepository,
        IUnitOfWork unitOfWork)
        : ICommandHandler<CreateConfigurationParameterCommand, ConfigurationParameterResponse>
    {
        public async Task<Result<ConfigurationParameterResponse>> Handle(CreateConfigurationParameterCommand request, CancellationToken cancellationToken)
        {
            await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);


            var result = ConfigurationParameter.Create(
                request.Uuid,
                request.Name,
                request.ParentId,
                request.Parent,
                request.Children,
                request.Status,
                request.Type,
                request.Editable,
                request.System,
                request.Metadata,
                request.HomologationCode
            );

            if (result.IsFailure)
            {
                return Result.Failure<ConfigurationParameterResponse>(result.Error);
            }

            var configurationparameter = result.Value;
            
            configurationparameterRepository.Insert(configurationparameter);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new ConfigurationParameterResponse(
                configurationparameter.ConfigurationParameterId,
                configurationparameter.Uuid,
                configurationparameter.Name,
                configurationparameter.ParentId,
                configurationparameter.Parent,
                configurationparameter.Children,
                configurationparameter.Status,
                configurationparameter.Type,
                configurationparameter.Editable,
                configurationparameter.System,
                configurationparameter.Metadata,
                configurationparameter.HomologationCode
            );
        }
    }
}