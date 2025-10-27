using Accounting.Integrations.AccountingGeneration;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;

namespace Accounting.Application.AccountingGeneration;

internal sealed class AccountingGenerationCommandHandler() : ICommandHandler<AccountingGenerationCommand, Unit>
{
    public Task<Result<Unit>> Handle(AccountingGenerationCommand request, CancellationToken cancellationToken)
    {
        try
        {


        }catch(Exception ex)
        {

        }
    }
}
