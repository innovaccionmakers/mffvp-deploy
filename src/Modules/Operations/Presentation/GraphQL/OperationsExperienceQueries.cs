using MediatR;
using Operations.Integrations.ConfigurationParameters;
using Operations.Integrations.Origins;
using Operations.Integrations.Banks;
using Operations.Presentation.DTOs;
using Operations.Integrations.SubTransactionTypes;

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
            x.Uuid,
            x.Name,
            x.Status,
            x.HomologatedCode
        )).ToList();
    }

    public async Task<IReadOnlyCollection<SubTransactionTypeDto>> GetSubTransactionTypesAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetSubTransactionTypesQuery(categoryId), cancellationToken);
        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve transaction subtypes.");
        }
        var transactionSubtypes = result.Value;
        return transactionSubtypes.Select(x => new SubTransactionTypeDto(
            x.SubtransactionTypeId.ToString(),
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

    public async Task<IReadOnlyCollection<BankDto>> GetBanksAsync(
        CancellationToken cancellationToken = default)
    {

        var result = await mediator.Send(new GetBanksQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve banks.");
        }

        var banks = result.Value;


        return banks.Select(x => new BankDto(
            x.BankId.ToString(),
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
}