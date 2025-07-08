using Associate.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;

namespace Associate.Presentation.GraphQL;

public interface IAssociatesExperienceMutations
{
    public Task<GraphqlMutationResult> RegisterActivateAsync(
        CreateActivateInput input,
        IValidator<CreateActivateInput> validator,
        CancellationToken cancellationToken
    );
    public Task<GraphqlMutationResult> UpdatePensionRequirementsAsync(
        UpdatePensionRequirementInput input,
        IValidator<UpdatePensionRequirementInput> validator,
        CancellationToken cancellationToken
    );
    public Task<GraphqlMutationResult> UpdateActivateAsync(
        UpdateActivateInput input,
        IValidator<UpdateActivateInput> validator,
        CancellationToken cancellationToken
    );
    public Task<GraphqlMutationResult> RegisterPensionRequirementsAsync(
        CreatePensionRequirementInput input,
        IValidator<CreatePensionRequirementInput> validator,
        CancellationToken cancellationToken
    );
}
