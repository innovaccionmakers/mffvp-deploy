using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.PreClosing.Services.Validation;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Commands.RunClosing;

internal sealed class PrepareClosingCommandHandler(
        IPrepareClosingOrchestrator closingOrchestrator,
        IUnitOfWork unitOfWork,
        ILogger<PrepareClosingCommandHandler> logger
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

            var result = await closingOrchestrator.PrepareAsync(command, cancellationToken);

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
