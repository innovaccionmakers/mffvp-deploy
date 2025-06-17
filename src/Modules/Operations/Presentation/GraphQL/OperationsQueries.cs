using MediatR;
using Operations.Integrations.DocumentTypes;
using Operations.Integrations.TransactionTypes;
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
}