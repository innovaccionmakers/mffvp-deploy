using Common.SharedKernel.Core.Primitives;

using MediatR;
using Operations.Integrations.ClientOperations.GetOperationsND;
using Operations.Integrations.ClientOperations.GetOperationsVoid;
using Operations.Integrations.ConfigurationParameters;
using Operations.Integrations.OperationTypes;
using Operations.Integrations.Origins;
using Operations.Presentation.DTOs;
using System.Text.Json;

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

    public async Task<IReadOnlyCollection<DebitNoteCauseDto>> GetDebitNoteCausesAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetDebitNoteCausesQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve debit note causes.");
        }

        var debitNoteCauses = result.Value;

        return debitNoteCauses.Select(x => new DebitNoteCauseDto(
            x.Id,
            x.Uuid,
            x.Name,
            x.Status,
            x.HomologatedCode
        )).ToList();
    }

    public async Task<IReadOnlyCollection<CancellationClauseDto>> GetCancellationClausesAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCancellationClauseQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve cancellation clauses.");
        }

        var cancellationClauses = result.Value;

        return cancellationClauses.Select(x => new CancellationClauseDto(
            x.ConfigurationParameterId,
            x.Uuid,
            x.Name,
            x.Status,
            x.HomologationCode
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

    public async Task<OperationNdPageDto> GetOperationsNdAsync(
        DateTime startDate,
        DateTime endDate,
        int affiliateId,
        int objectiveId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetOperationsNDQuery(startDate, endDate, affiliateId, objectiveId, pageNumber, pageSize),
            cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve ND operations.");
        }

        var items = result.Value.Items
            .Select(item => new OperationNdDto(
                item.ClientOperationId,
                item.ProcessDate,
                item.TransactionTypeName,
                item.Amount,
                item.ContingentWithholding))
            .ToList();

        return new OperationNdPageDto(
            result.Value.PageNumber,
            result.Value.PageSize,
            result.Value.TotalCount,
            result.Value.TotalPages,
            items);
    }

    public async Task<OperationVoidPageDto> GetOperationsVoidAsync(
        int affiliateId,
        int objectiveId,
        long operationTypeId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetOperationsVoidQuery(affiliateId, objectiveId, operationTypeId, pageNumber, pageSize),
            cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve void operations.");
        }

        var items = result.Value.Items
            .Select(item => new OperationVoidDto(
                item.ClientOperationId,
                item.ProcessDate,
                item.TransactionTypeName,
                item.OperationTypeId,
                item.Amount,
                item.ContingentWithholding))
            .ToList();

        return new OperationVoidPageDto(
            result.Value.PageNumber,
            result.Value.PageSize,
            result.Value.TotalCount,
            result.Value.TotalPages,
            items);
    }

    public async Task<IReadOnlyCollection<AccTransactionTypesDto>> GetAccTransactionTypesAsync(CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAccTransactionTypesQuery(), cancellationToken);
        if (!result.IsSuccess || result.Value == null)
            throw new InvalidOperationException("Failed to retrieve operation types.");

        var operationTypes = result.Value;
        return operationTypes.Select(x => new AccTransactionTypesDto(
            x.OperationTypeId,
            x.Name,
            x.CategoryId,
            x.Nature,
            x.Status,
            x.External,
            x.Visible,
            x.AdditionalAttributes,
            x.HomologatedCode
        )).ToList();
    }

}
