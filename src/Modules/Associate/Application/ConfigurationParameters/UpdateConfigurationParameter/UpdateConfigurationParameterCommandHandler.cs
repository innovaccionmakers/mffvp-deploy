using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Associate.Domain.ConfigurationParameters;
using Associate.Integrations.ConfigurationParameters.UpdateConfigurationParameter;
using Associate.Integrations.ConfigurationParameters;
using Associate.Application.Abstractions.Data;

namespace Associate.Application.ConfigurationParameters;
internal sealed class UpdateConfigurationParameterCommandHandler(
    IConfigurationParameterRepository configurationparameterRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateConfigurationParameterCommand, ConfigurationParameterResponse>
{
    public async Task<Result<ConfigurationParameterResponse>> Handle(UpdateConfigurationParameterCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await configurationparameterRepository.GetAsync(request.ConfigurationParameterId, cancellationToken);
        if (entity is null)
        {
            return Result.Failure<ConfigurationParameterResponse>(ConfigurationParameterErrors.NotFound(request.ConfigurationParameterId));
        }

        entity.UpdateDetails(
            request.NewUuid, 
            request.NewName, 
            request.NewParentId, 
            request.NewParent, 
            request.NewChildren, 
            request.NewStatus, 
            request.NewType, 
            request.NewEditable, 
            request.NewSystem, 
            request.NewMetadata, 
            request.NewHomologationCode
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ConfigurationParameterResponse(entity.ConfigurationParameterId, entity.Uuid, entity.Name, entity.ParentId, entity.Parent, entity.Children, entity.Status, entity.Type, entity.Editable, entity.System, entity.Metadata, entity.HomologationCode);
    }
}