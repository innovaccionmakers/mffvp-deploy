

using Closing.Application.PreClosing.Services.Validation.Dto;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.Validation;

public interface IRunSimulationValidationReader
{
    Task<Result<RunSimulationValidationInfo>> ValidateAndDescribeAsync(
        RunSimulationCommand command,
        CancellationToken cancellationToken = default);
}