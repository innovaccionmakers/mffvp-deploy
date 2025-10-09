
using Common.SharedKernel.Application.Rpc;

using MediatR;
using Operations.Integrations.Contributions.ProcessPendingContributions;


namespace Operations.IntegrationEvents.PendingContributionProcessor;

public sealed class PendingContributionProcessor(ISender mediator) : IRpcHandler<ProcessPendingTransactionsRequest, ProcessPendingTransactionsResponse>
{
     public async Task<ProcessPendingTransactionsResponse> HandleAsync(ProcessPendingTransactionsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ProcessPendingContributionsCommand(request.PortfolioId, request.ProcessDate);
            await mediator.Send(command, cancellationToken);

            return new ProcessPendingTransactionsResponse(
                  Succeeded: true,
                  Status: "Processed",
                  AppliedCount: 0,
                  SkippedCount: 0,
                  Code: "OK",
                  Message: "Procesamiento completado."
              );
        }
        catch (OperationCanceledException)
        {

            return new ProcessPendingTransactionsResponse(
                Succeeded: false,
                Status: "Error",
                AppliedCount: 0,
                SkippedCount: 0,
                Code: "OPS-CANCELED",
                Message: "Operación cancelada."
            );
        }
        catch (Exception ex)
        {

            return new ProcessPendingTransactionsResponse(
                Succeeded: false,
                Status: "Error",
                AppliedCount: 0,
                SkippedCount: 0,
                Code: "OPS-UNHANDLED",
                Message: ex.Message
            );
        }
    }
}