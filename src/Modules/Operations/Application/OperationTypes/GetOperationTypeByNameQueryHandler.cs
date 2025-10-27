using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Operations.Domain.OperationTypes;
using Operations.Integrations.OperationTypes;
using Common.SharedKernel.Core.Primitives;

namespace Operations.Application.OperationTypes;

public class GetOperationTypeByNameQueryHandler(IOperationTypeRepository repository, ILogger<GetOperationTypeByNameQueryHandler> logger) : IQueryHandler<GetOperationTypeByNameQuery, IReadOnlyCollection<OperationTypeResponse>>
{
    public async Task<Result<IReadOnlyCollection<OperationTypeResponse>>> Handle(GetOperationTypeByNameQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var operationType = await repository.GetByNameAsync(request.Name, cancellationToken);
            if (operationType is null)
            {
                logger.LogWarning("No se encontró el tipo de operación con nombre {Name}.", request.Name);
                return Result.Failure<IReadOnlyCollection<OperationTypeResponse>>(new Error("Error", $"No se encontró el tipo de operación con nombre {request.Name}.", ErrorType.NotFound));
            }

            var listFromCache = operationType
                    .Select(c => new OperationTypeResponse(
                        c.OperationTypeId,
                        c.Name,
                        c.CategoryId.ToString(),
                        c.Nature,
                        c.Status,
                        c.External,
                        c.HomologatedCode))
                    .ToList();

            return Result.Success((IReadOnlyCollection<OperationTypeResponse>)listFromCache);

        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Ocurrió un error inesperado al obtener el tipo de operación por nombre.");
            return Result.Failure<IReadOnlyCollection<OperationTypeResponse>>(new Error("Error", "Ocurrió un error inesperado al obtener el tipo de operación por nombre.", ErrorType.Problem));
        }     
    }
}