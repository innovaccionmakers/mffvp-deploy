using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Operations.Domain.OperationTypes;
using Operations.Integrations.OperationTypes;
using Common.SharedKernel.Core.Primitives;

namespace Operations.Application.OperationTypes;

public class GetOperationTypeByNameQueryHandler(IOperationTypeRepository repository, ILogger<GetOperationTypeByNameQueryHandler> logger) : IQueryHandler<GetOperationTypeByNameQuery, OperationType>
{
    public async Task<Result<OperationType>> Handle(GetOperationTypeByNameQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var operationType = await repository.GetByNameAsync(request.Name, cancellationToken);
            if (operationType is null)
            {
                logger.LogWarning("No se encontró el tipo de operación con nombre {Name}.", request.Name);
                return Result.Failure<OperationType>(new Error("Error", $"No se encontró el tipo de operación con nombre {request.Name}.", ErrorType.NotFound));
            }
            return Result.Success(operationType);

        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Ocurrió un error inesperado al obtener el tipo de operación por nombre.");
            return Result.Failure<OperationType>(new Error("Error", "Ocurrió un error inesperado al obtener el tipo de operación por nombre.", ErrorType.Problem));
        }     
    }
}