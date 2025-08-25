using Associate.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;

namespace Associate.Presentation.GraphQL;

public interface IAssociatesExperienceMutations
{
    public Task<GraphqlResult> RegisterActivateAsync(
        CreateActivateInput input,
        IValidator<CreateActivateInput> validator,
        CancellationToken cancellationToken
    );
    public Task<GraphqlResult> UpdatePensionRequirementsAsync(
        UpdatePensionRequirementInput input,
        IValidator<UpdatePensionRequirementInput> validator,
        CancellationToken cancellationToken
    );
    public Task<GraphqlResult> UpdateActivateAsync(
        UpdateActivateInput input,
        IValidator<UpdateActivateInput> validator,
        CancellationToken cancellationToken
    );
    public Task<GraphqlResult> RegisterPensionRequirementsAsync(
        CreatePensionRequirementInput input,
        IValidator<CreatePensionRequirementInput> validator,
        CancellationToken cancellationToken
    );
}
