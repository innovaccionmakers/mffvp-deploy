using Accounting.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;

namespace Accounting.Presentation.GraphQL
{
    public interface IConceptMutations
    {
        Task<GraphqlResult> CreateConceptAsync(CreateConceptInput input, IValidator<CreateConceptInput> validator, CancellationToken cancellationToken = default);
        Task<GraphqlResult> UpdateConceptAsync(UpdateConceptInput input, IValidator<UpdateConceptInput> validator, CancellationToken cancellationToken = default);
        Task<GraphqlResult> DeleteConceptAsync(DeleteConceptInput input, IValidator<DeleteConceptInput> validator, CancellationToken cancellationToken = default);
    }
}

