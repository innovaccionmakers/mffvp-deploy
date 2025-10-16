using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using Operations.Presentation.DTOs;
using Operations.Presentation.GraphQL.Inputs;

namespace Operations.Presentation.GraphQL;

public interface IOperationsExperienceMutation
{
    public Task<GraphqlResult<ContributionMutationResult>> RegisterContributionAsync(
        CreateContributionInput input,
        IValidator<CreateContributionInput> validator,
        CancellationToken cancellationToken = default
    );

    public Task<GraphqlResult<DebitNoteMutationResult>> RegisterDebitNoteAsync(
        CreateDebitNoteInput input,
        IValidator<CreateDebitNoteInput> validator,
        CancellationToken cancellationToken = default
    );
}
