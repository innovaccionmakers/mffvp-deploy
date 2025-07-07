
using Closing.Application.Abstractions;
using Closing.Application.Abstractions.Data;
using Closing.Application.Abstractions.External;
using Closing.Application.PreClosing.Services.Orchestation;
using Closing.Domain.ProfitLossConcepts;
using Closing.Domain.ProfitLosses;
using Closing.Integrations.PreClosingSimulation.RunSimulation;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.RunSimulation
{
    internal sealed class RunSimulationCommandHandler(
    ISimulationOrchestrator _simulationOrchestrator,
    IInternalRuleEvaluator<ClosingModuleMarker> ruleEvaluator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RunSimulationCommand, bool>
    {
        private const string WorkflowName = "Closing.PreClosingSimulation.RunSimulation";
        public async Task<Result<bool>> Handle(RunSimulationCommand command, CancellationToken cancellationToken)
        {

            var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                await _simulationOrchestrator.RunSimulationAsync(command, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

         

            return Result.Success(true, "La operación se realizó exitosamente.");
        }
    }
}
