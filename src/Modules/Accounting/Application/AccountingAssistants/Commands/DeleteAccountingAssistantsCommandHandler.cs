using Accounting.Domain.AccountingAssistants;
using Accounting.Integrations.AccountingAssistants.Commands;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AccountingAssistants.Commands
{
    internal class DeleteAccountingAssistantsCommandHandler(
        ILogger<DeleteAccountingAssistantsCommandHandler> logger,
        IAccountingAssistantRepository accountingAssistantRepository): ICommandHandler<DeleteAccountingAssistantsCommand, bool>
    {
        public async Task<Result<bool>> Handle(DeleteAccountingAssistantsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await accountingAssistantRepository.DeleteRangeAsync(cancellationToken);

                logger.LogInformation("Registros de auxiliar contable eliminados exitosamente.");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al eliminar los registros de auxiliar contable.");
                return Result.Failure<bool>(new Error("Exception", ex.Message, ErrorType.Failure));
            }
        }
    }
}
