using System.Threading;
using System.Threading.Tasks;
using Closing.Integrations.PreClosing.RunSimulation;
using Closing.Presentation.DTOs;
using Closing.Presentation.GraphQL.DTOs;
using Closing.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;

namespace Closing.Presentation.GraphQL;

public interface IClosingExperienceMutations
{
    Task<GraphqlMutationResult<LoadProfitLossResult>> LoadProfitLossAsync(
        LoadProfitLossInput input,
        IValidator<LoadProfitLossInput> validator,
        CancellationToken cancellationToken = default
    );

    Task<GraphqlMutationResult<RunSimulationDto>> RunSimulationAsync(
        RunSimulationInput input,
        IValidator<RunSimulationInput> validator,
        CancellationToken cancellationToken = default
    );
}