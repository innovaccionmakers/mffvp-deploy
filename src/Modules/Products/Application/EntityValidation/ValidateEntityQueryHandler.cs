using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions;
using Products.Domain.Administrators;
using Products.Integrations.EntityValidation;

namespace Products.Application.EntityValidation;

internal sealed class ValidateEntityQueryHandler(
    IAdministratorRepository administratorRepository,
    IRuleEvaluator<ProductsModuleMarker> ruleEvaluator)
    : IQueryHandler<ValidateEntityQuery, bool>
{
    private const string ValidationWorkflow = "Products.Entity.Validation";

    public async Task<Result<bool>> Handle(ValidateEntityQuery request, CancellationToken cancellationToken)
    {
        var entityExists = await administratorRepository.ExistsByEntityCodeAsync(
            request.Entity,
            cancellationToken);

        var validationContext = new { EntityExists = entityExists };

        var (isValid, _, errors) = await ruleEvaluator
            .EvaluateAsync(ValidationWorkflow, validationContext, cancellationToken);

        if (!isValid)
        {
            var first = errors.First();
            return Result.Failure<bool>(Error.Validation(first.Code, first.Message));
        }

        return true;
    }
}

