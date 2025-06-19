using MediatR;
using Operations.Integrations.ConfigurationParameters;
using Operations.Presentation.DTOs;

namespace Operations.Presentation.GraphQL;

[ExtendObjectType("Query")]
public class OperationsQueries
{
    public async Task<IReadOnlyCollection<TransactionTypeDto>> GetTransactionTypesAsync(
        [Service] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
            var result = await mediator.Send(new GetTransactionTypesQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve transaction types.");
        }

        var transactionTypes = result.Value;
        
        return transactionTypes.Select(x => new TransactionTypeDto(
            x.TransactionTypeId.ToString(),
            x.Name,
            x.Status,
            x.HomologatedCode,
            x.SubtransactionTypes.Select(st => new TransactionSubtypesDto
            (
                st.SubtransactionTypeId.ToString(),
                st.Name,
                st.HomologatedCode
            )).ToList()
        )).ToList();
    }

    public async Task<IReadOnlyCollection<DocumentTypeDto>> GetDocumentTypesAsync(
        [Service] IMediator mediator,
        CancellationToken cancellationToken = default)
    {

        var result = await mediator.Send(new GetDocumentTypesQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve transaction types.");
        }

        var documentTypes = result.Value;


        return documentTypes.Select(x => new DocumentTypeDto(
            x.DocumentTypeId.ToString(),
            x.Name,
            x.Status,
            x.HomologatedCode
        )).ToList();

    }

    public async Task<IReadOnlyCollection<CertificationStatusDto>> GetCertificationStatusesAsync(
        [Service] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCertificationStatusesQuery(), cancellationToken);
        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve certification statuses.");
        }
        var certificationStatuses = result.Value;
        return certificationStatuses.Select(x => new CertificationStatusDto(
            x.CertificationStatusId.ToString(),
            x.Name,
            x.Status,
            x.HomologatedCode
        )).ToList();
    }

    public async Task<IReadOnlyCollection<OriginModeDto>> GetOriginModesAsync(
        [Service] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetOriginModesQuery(), cancellationToken);
        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve origin modes.");
        }
        var originModes = result.Value;
        return originModes.Select(x => new OriginModeDto(
            x.OriginModeId.ToString(),
            x.Name,
            x.Status,
            x.HomologatedCode
        )).ToList();
    }

    public async Task<IReadOnlyCollection<CollectionMethodDto>> GetCollectionMethodsAsync(
        [Service] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCollectionMethodsQuery(), cancellationToken);
        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve collection methods.");
        }
        var collectionMethods = result.Value;
        return collectionMethods.Select(x => new CollectionMethodDto(
            x.CollectionMethodId.ToString(),
            x.Name,
            x.Status,
            x.HomologatedCode
        )).ToList();
    }

    public async Task<IReadOnlyCollection<PaymentMethodDto>> GetPaymentMethodsAsync(
        [Service] IMediator mediator,
        CancellationToken cancellationToken = default
    ){
        var result = await mediator.Send(new GetPaymentMethodsQuery(), cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            throw new InvalidOperationException("Failed to retrieve payment methods");
        }

        var paymentMethods = result.Value;

        return paymentMethods.Select(x => new PaymentMethodDto(
            x.PaymentMethodId.ToString(),
            x.Name,
            x.Status,
            x.HomologatedCode
        ))

    }
}