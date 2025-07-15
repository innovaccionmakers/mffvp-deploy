using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.Validation
{
    public interface IBusinessValidator<T>
    {
        public Task<Result> ValidateAsync(RunSimulationCommand command, CancellationToken ct);
    }
}