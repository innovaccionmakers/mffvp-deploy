using Common.SharedKernel.Core.Primitives;

using MediatR;
using Operations.Integrations.ClientOperations.GetClientOperationsByProcessDate;
using Operations.Integrations.ConfigurationParameters;
using Operations.Integrations.OperationTypes;
using Operations.Integrations.Origins;
using Operations.Presentation.DTOs;

namespace Operations.Presentation.GraphQL;
    
public class OperationsExperienceQueries(IMediator mediator) : IOperationsExperienceQueries
{
    public async Task<IReadOnlyCollection<TransactionTypeDto>> GetTransactionTypesAsync(        
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetTransactionTypesQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve transaction types.");
        }

        var transactionTypes = result.Value;

        return transactionTypes.Select(x => new TransactionTypeDto(
            x.OperationTypeId.ToString(),
            x.Name,
            x.Status == Status.Active,
            x.HomologatedCode
        )).ToList();
    }

    public async Task<IReadOnlyCollection<OperationTypeDto>> GetOperationTypesAsync(
        int? categoryId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetOperationTypesByCategoryQuery(categoryId), cancellationToken);
        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve operation types.");
        }
        var operationTypes = result.Value;
        return operationTypes.Select(x => new OperationTypeDto(
            x.OperationTypeId.ToString(),
            x.Name,
            x.HomologatedCode
        )).ToList();
    }

    public async Task<IReadOnlyCollection<CertificationStatusDto>> GetCertificationStatusesAsync(        
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCertificationStatusesQuery(), cancellationToken);
        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve certification statuses.");
        }
        var certificationStatuses = result.Value;
        return certificationStatuses.Select(x => new CertificationStatusDto(
            x.Uuid,
            x.Name,
            x.Status,
            x.HomologatedCode
        )).ToList();
    }

    public async Task<IReadOnlyCollection<OriginModeDto>> GetOriginModesAsync(
        int originId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetOriginModesQuery(originId), cancellationToken);
        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve origin modes.");
        }
        var originModes = result.Value;
        return originModes.Select(x => new OriginModeDto(
            x.Uuid,
            x.Name,
            x.Status,
            x.HomologatedCode
        )).ToList();
    }

    public async Task<IReadOnlyCollection<CollectionMethodDto>> GetCollectionMethodsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCollectionMethodsQuery(), cancellationToken);
        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve collection methods.");
        }
        var collectionMethods = result.Value;
        return collectionMethods.Select(x => new CollectionMethodDto(
            x.Uuid,
            x.Name,
            x.Status,
            x.HomologatedCode
        )).ToList();
    }

    public async Task<IReadOnlyCollection<PaymentMethodDto>> GetPaymentMethodsAsync(        
        CancellationToken cancellationToken = default
    ){
        var result = await mediator.Send(new GetPaymentMethodsQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve payment methods");
        }

        var paymentMethods = result.Value;

        return paymentMethods.Select(x => new PaymentMethodDto(
            x.Uuid,
            x.Name,
            x.Status,
            x.HomologatedCode
        )).ToList();

    }

    public async Task<IReadOnlyCollection<OriginContributionDto>> GetOriginContributionsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetOriginContributionsQuery(), cancellationToken);
        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve origin contributions.");
        }
        var originContributions = result.Value;
        return originContributions.Select(x => new OriginContributionDto(
            x.OriginId,
            x.Name,
            x.HomologatedCode
        )).ToList();
    }

    public async Task<string> GetWithholdingContingencyAsync(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetWithholdingContingencyQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve withholding contingency.");
        }

        return result.Value;
    }

    public async Task<IReadOnlyCollection<ClientOperationsByProcessDateDto>> GetClientOperationsByProcessDateAsync(DateTime processDate, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetClientOperationsByProcessDateQuery(processDate), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
            throw new InvalidOperationException("Failed to retrieve client operations.");

        var response = result.Value
        .Select(c => new ClientOperationsByProcessDateDto(
            c.Amount,
            c.CollectionAccount,
            c.PaymentMethodDetail,
            c.Name))
        .ToList();

        return response;
    }
}