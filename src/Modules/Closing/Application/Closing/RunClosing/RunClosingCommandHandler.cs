using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Services.Orchestation;
using Closing.Application.PreClosing.Services.Validation;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.RunClosing;

internal sealed class RunClosingCommandHandler(
        IClosingOrchestrator closingOrchestrator,
        IUnitOfWork unitOfWork,
        ILogger<RunClosingCommandHandler> logger
       )
        : ICommandHandler<RunClosingCommand, ClosedResult>
{
    public async Task<Result<ClosedResult>> Handle(RunClosingCommand command, CancellationToken cancellationToken)
    {
        var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Validación de reglas de negocio antes de orquestar
            //var validationResult = await businessValidator.ValidateAsync(command, cancellationToken);
            //if (validationResult.IsFailure)
            //{
            //    return Result.Failure<ClosedResult>(validationResult.Error);
            //}

            var result = await closingOrchestrator.RunClosingAsync(command, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger?.LogError(ex, "Error en RunClosingCommand para Portafolio {PortfolioId} - Fecha {Date}",
                command.PortfolioId, command.ClosingDate);
            throw;
        }
    }
}
