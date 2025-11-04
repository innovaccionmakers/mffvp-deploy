using Accounting.Application.Abstractions;
using Accounting.Domain.Constants;
using Accounting.IntegrationEvents.AccountingProcess;
using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountingConcepts;
using Accounting.Integrations.AccountingFees;
using Accounting.Integrations.AccountingOperations;
using Accounting.Integrations.AccountingReturns;
using Accounting.Integrations.AccountProcess;
using Accounting.Integrations.AutomaticConcepts;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Core.Primitives;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Accounting.Application.Abstractions.External;

namespace Accounting.Application.AccountProcess;

internal sealed class AccountProcessHandler(
    ISender sender,
    IClosingExecutionStore closingValidator,
    IServiceProvider serviceProvider,
    IAccountingNotificationService accountingNotificationService,
    IUserLocator userLocator,
    IUserService userService) : ICommandHandler<AccountProcessCommand, AccountProcessResult>
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

        var accountingFeesCommand = new AccountingFeesCommand(command.PortfolioIds, processDate);
        var accountingReturnsCommand = new AccountingReturnsCommand(command.PortfolioIds, processDate);
        var acountingOperationsCommand = new AccountingOperationsCommand(command.PortfolioIds, processDate);
        var accountingConceptsCommand = new AccountingConceptsCommand(command.PortfolioIds, processDate);
        var automaticConceptsCommand = new AutomaticConceptsCommand(command.PortfolioIds, processDate);

        _ = Task.Run(async () => await ExecuteAccountingOperationWithScopeAsync(username, email, ProcessTypes.AccountingFees, accountingFeesCommand, processId, startDate, processDate, command.PortfolioIds, cancellationToken), cancellationToken);
        _ = Task.Run(async () => await ExecuteAccountingOperationWithScopeAsync(username, email, ProcessTypes.AccountingReturns, accountingReturnsCommand, processId, startDate, processDate, command.PortfolioIds, cancellationToken), cancellationToken);
        _ = Task.Run(async () => await ExecuteAccountingOperationWithScopeAsync(username, email, ProcessTypes.AccountingOperations, acountingOperationsCommand, processId, startDate, processDate, command.PortfolioIds, cancellationToken), cancellationToken);
        _ = Task.Run(async () => await ExecuteAccountingOperationWithScopeAsync(username, email, ProcessTypes.AccountingConcepts, accountingConceptsCommand, processId, startDate, processDate, command.PortfolioIds, cancellationToken), cancellationToken);
        _ = Task.Run(async () => await ExecuteAccountingOperationWithScopeAsync(username, email, ProcessTypes.AutomaticConcepts, automaticConceptsCommand, processId, startDate, processDate, command.PortfolioIds, cancellationToken), cancellationToken);

        return Result.Success(new AccountProcessResult(processId));
    }

    private async Task ExecuteAccountingOperationWithScopeAsync<T>(
        string user,
        string? email,
        string operationType,
        T command,
        string processId,
        DateTime startDate,
        DateTime processDate,
        IEnumerable<int> portfolioIds,
        CancellationToken cancellationToken) where T : ICommand<bool>
    {
        using var scope = serviceProvider.CreateScope();
        var scopedSender = scope.ServiceProvider.GetRequiredService<ISender>();
        var scopedEventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
        var scopedUserService = scope.ServiceProvider.GetRequiredService<IUserService>();


        try
        {
            var result = await scopedSender.Send(command, cancellationToken);

            var success = result.IsSuccess;
            var message = success
                ? $"{operationType} procesado exitosamente"
                : $"{result.Error?.Description ?? "falló durante el procesamiento"}";


            await PublishProcessCompletedAsync(
                user,
                email,
                operationType,
                success,
                success ? null : message,
                processId,
                startDate,
                processDate,
                portfolioIds,
                scopedEventBus,
                cancellationToken);
        }
        catch (Exception ex)
        {
            await PublishProcessCompletedAsync(
                user,
                email,
                operationType,
                false,
                $"Excepción durante el procesamiento: {ex.Message}",
                processId,
                startDate,
                processDate,
                portfolioIds,
                scopedEventBus,
                cancellationToken);
        }
    }

    private async Task PublishProcessCompletedAsync(
        string user,
        string email,
        string processType,
        bool isSuccess,
        string? errorMessage,
        string processId,
        DateTime startDate,
        DateTime processDate,
        IEnumerable<int> portfolioIds,
        IEventBus eventBus,
        CancellationToken cancellationToken)
    {
        var evt = new AccountingProcessCompletedIntegrationEvent(
            user,
            email,
            processType,
            isSuccess,
            errorMessage,
            processId,
            startDate,
            processDate,
            portfolioIds);

        await eventBus.PublishAsync(evt, cancellationToken);
    }

}
