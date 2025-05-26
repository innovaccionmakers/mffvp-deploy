using Common.SharedKernel.Domain;
using Products.Application.Abstractions;
using Products.Application.Abstractions.Rules;
using Products.Domain.Portfolios;
using Products.Integrations.ContributionValidation;

namespace Products.Application.ContributionValidation;

internal sealed class ContributionValidationQueryHandler(
    IPortfolioRepository portfolioRepository,
    IRuleEvaluator<ProductsModuleMarker> ruleEvaluator)
{
    private const string ValidationWorkflow = "Products.Contributions.Validation";

    public async Task<Result<ContributionValidationResponse>> Handle(
        ContributionValidationQuery request,
        CancellationToken cancellationToken)
    {
        Portfolio? existsByStandardCode = null;

        if (!string.IsNullOrEmpty(request.PortfolioStandardCode))
        {
            existsByStandardCode = await portfolioRepository
                .GetByStandardCodeAsync(request.PortfolioStandardCode, cancellationToken);
        }
        else
        {
            // TODO: lógica alternativa para identificar portafolio
        }

        var validationContext = new
        {
            request.ObjectiveId,
            request.PortfolioStandardCode,
            request.DepositDate,
            request.ExecutionDate,
            request.Amount,
            ExistsByStandardCode = existsByStandardCode
        };

        var (validationSucceeded, _, validationErrors) = await ruleEvaluator
            .EvaluateAsync(ValidationWorkflow, validationContext, cancellationToken);

        if (!validationSucceeded)
        {
            var firstError = validationErrors.First();
            return Result.Failure<ContributionValidationResponse>(
                Error.Validation(firstError.Code, firstError.Message));
        }

        return Result.Success(new ContributionValidationResponse(true));
    }
}