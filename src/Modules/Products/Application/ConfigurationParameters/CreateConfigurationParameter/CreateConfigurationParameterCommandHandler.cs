using System.Text.Json;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Data;
using Products.Domain.ConfigurationParameters;
using Products.Integrations.ConfigurationParameters;
using Products.Integrations.ConfigurationParameters.CreateConfigurationParameter;

namespace Products.Application.ConfigurationParameters.CreateConfigurationParameter;

internal sealed class CreateConfigurationParameterCommandHandler(
    IConfigurationParameterRepository repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateConfigurationParameterCommand, ConfigurationParameterResponse>
{
    public async Task<Result<ConfigurationParameterResponse>> Handle(
        CreateConfigurationParameterCommand request,
        CancellationToken cancellationToken)
    {
        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var parameter = ConfigurationParameter.Create(
            request.Name,
            request.HomologationCode,
            request.Type,
            request.ParentId,
            request.Status,
            request.Editable,
            request.System,
            request.Metadata ?? JsonDocument.Parse("{}")
        );

        repository.Insert(parameter);

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