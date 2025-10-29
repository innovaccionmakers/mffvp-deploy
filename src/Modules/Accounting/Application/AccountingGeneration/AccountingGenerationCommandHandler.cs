using Accounting.Domain.AccountingAssistants;
using Accounting.Integrations.AccountingGeneration;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;

namespace Accounting.Application.AccountingGeneration;

internal sealed class AccountingGenerationCommandHandler( IAccountingAssistantRepository accountingAssistantRepository) : ICommandHandler<AccountingGenerationCommand, Unit>
{
    public async Task<Result<Unit>> Handle(AccountingGenerationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var accountingAssistants = await accountingAssistantRepository.GetAllAsync(cancellationToken);

        }catch(Exception ex)
        {

        }
        return Result.Success(Unit.Value);
    }
}
