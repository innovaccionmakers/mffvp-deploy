using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.Constants;
using Accounting.IntegrationEvents.AccountingProcess;
using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountProcess;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;

namespace Accounting.Application.AccountProcess;

internal sealed class AccountProcessHandler(
    ISender sender,
    IClosingExecutionStore closingValidator,
    IAccountingNotificationService accountingNotificationService,
    IUserLocator userLocator,
    IUserService userService,
    IEventBus eventBus) : ICommandHandler<AccountProcessCommand, AccountProcessResult>
{
    private const string ProcessIdPrefix = "CONTAFVP";

    public async Task<Result<AccountProcessResult>> Handle(AccountProcessCommand command, CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow;

        var isActive = await closingValidator.IsClosingActiveAsync(cancellationToken);
        if (isActive)
            return Result.Failure<AccountProcessResult>(new Error("0001", "Existe un proceso de cierre activo.", ErrorType.Validation));

        var deleteCommand = new DeleteAccountingAssistantsCommand();
        var deleteResult = await sender.Send(deleteCommand, cancellationToken);
        if (deleteResult.IsFailure)
            return Result.Failure<AccountProcessResult>(deleteResult.Error);

        var username = userService.GetUserName();
        var email = (await userLocator.GetEmailUserAsync(username, cancellationToken))?.Value;

        var processId = $"{ProcessIdPrefix}{DateTime.UtcNow:yyyyMMddHHmmss}";
        var processDate = command.ProcessDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        await accountingNotificationService.SendProcessInitiatedAsync(
            username,
            email,
            processId.ToString(),
            processDate,
            cancellationToken
       );
        var capPublisher = eventBus.GetCapPublisher();

        var processTypes = new[]
        {
            ProcessTypes.AccountingFees,
            ProcessTypes.AccountingReturns,
            ProcessTypes.AccountingOperations,
            ProcessTypes.AccountingConcepts,
            ProcessTypes.AutomaticConcepts
        };

        foreach (var processType in processTypes)
        {
            var operationEvent = new AccountingOperationRequestedIntegrationEvent(
                username,
                email,
                processType,
                processId,
                startDate,
                processDate,
                command.PortfolioIds);

            await capPublisher.PublishAsync(nameof(AccountingOperationRequestedIntegrationEvent), operationEvent, cancellationToken: cancellationToken);
        }

        return Result.Success(new AccountProcessResult(processId));
    }
}
