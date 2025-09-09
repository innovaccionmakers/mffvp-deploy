using System.Threading;
using System.Threading.Tasks;
using Closing.Integrations.PreClosing.RunSimulation;
using Closing.Presentation.GraphQL.DTOs;
using Closing.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;

namespace Closing.Presentation.GraphQL;

public interface IClosingExperienceMutations
{
    Task<GraphqlResult<LoadProfitLossResult>> LoadProfitLossAsync(
        LoadProfitLossInput input,
        IValidator<LoadProfitLossInput> validator,
        CancellationToken cancellationToken = default
    );

    Task<GraphqlResult<RunSimulationDto>> RunSimulationAsync(
        RunSimulationInput input,
        IValidator<RunSimulationInput> validator,
        CancellationToken cancellationToken = default
    );

    Task<GraphqlResult<RunClosingDto>> RunClosingAsync(
        RunClosingInput input,
        IValidator<RunClosingInput> validator,
        CancellationToken cancellationToken = default);

    Task<GraphqlResult<ConfirmClosingDto>> ConfirmClosingAsync(
        ConfirmClosingInput input,
        IValidator<ConfirmClosingInput> validator,
        CancellationToken cancellationToken = default);

    Task<GraphqlResult<CancelClosingDto>> CancelClosingAsync(
        CancelClosingInput input,
        IValidator<CancelClosingInput> validator,
        CancellationToken cancellationToken = default);
}