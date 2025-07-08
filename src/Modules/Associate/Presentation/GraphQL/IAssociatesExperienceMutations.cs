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
}
