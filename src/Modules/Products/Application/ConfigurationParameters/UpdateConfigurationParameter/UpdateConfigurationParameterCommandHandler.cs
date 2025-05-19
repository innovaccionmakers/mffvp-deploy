using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Data;
using Products.Domain.ConfigurationParameters;
using Products.Integrations.ConfigurationParameters;
using Products.Integrations.ConfigurationParameters.UpdateConfigurationParameter;

namespace Products.Application.ConfigurationParameters.UpdateConfigurationParameter;

internal sealed class UpdateConfigurationParameterCommandHandler(
    IConfigurationParameterRepository repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateConfigurationParameterCommand, ConfigurationParameterResponse>
{
    public async Task<Result<ConfigurationParameterResponse>> Handle(
        UpdateConfigurationParameterCommand request,
        CancellationToken cancellationToken)
    {
        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var parameter = await repository.GetAsync(request.ConfigurationParameterId, cancellationToken);
        if (parameter is null)
            return Result.Failure<ConfigurationParameterResponse>(
                ConfigurationParameterErrors.NotFound(request.ConfigurationParameterId));

        parameter.UpdateDetails(
            request.NewName ?? parameter.Name,
            request.NewHomologationCode ?? parameter.HomologationCode,
            request.NewType ?? parameter.Type,
            request.NewParentId ?? parameter.ParentId,
            request.NewStatus ?? parameter.Status,
            request.NewEditable ?? parameter.Editable,
            request.NewSystem ?? parameter.System,
            request.NewMetadata ?? parameter.Metadata
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return new ConfigurationParameterResponse(
            parameter.ConfigurationParameterId,
            parameter.Uuid,
            parameter.Name,
            parameter.ParentId,
            parameter.Status,
            parameter.Type,
            parameter.Editable,
            parameter.System,
            parameter.Metadata,
            parameter.HomologationCode
        );
    }
}