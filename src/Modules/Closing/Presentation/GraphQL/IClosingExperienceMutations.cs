using System.Threading;
using System.Threading.Tasks;
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

    Task<GraphqlMutationResult> RunSimulationAsync(
        RunSimulationInput input,
        IValidator<RunSimulationInput> validator,
        CancellationToken cancellationToken = default
    );
}