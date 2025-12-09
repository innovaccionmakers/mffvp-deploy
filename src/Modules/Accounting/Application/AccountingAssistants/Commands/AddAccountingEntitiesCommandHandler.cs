using Accounting.Domain.AccountingAssistants;
using Common.SharedKernel.Core.Primitives;
using Accounting.Application.Abstractions.Data;
using Accounting.Integrations.AccountingAssistants.Commands;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AccountingAssistants.Commands;

internal sealed class AddAccountingEntitiesCommandHandler(ILogger<AddAccountingEntitiesCommandHandler> logger,
                                                 IAccountingAssistantRepository accountingAssistantRepository,
                                                 IUnitOfWork unitOfWork) : ICommandHandler<AddAccountingEntitiesCommand, bool>
{
    public async Task<Result<bool>> Handle(AddAccountingEntitiesCommand request, CancellationToken cancellationToken)
    {
        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await accountingAssistantRepository.AddRangeAsync(request.AccountingAssistants, cancellationToken);
            await tx.CommitAsync(cancellationToken);

            logger.LogInformation("Se agregaron con éxito {Count} entidades contables", request.AccountingAssistants.Count());
            return true;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Ocurrio un error Inesperado al guardar la información contable");
            return Result.Failure<bool>(new Error("Exception", ex.Message, ErrorType.Failure));
        }
    }

}
