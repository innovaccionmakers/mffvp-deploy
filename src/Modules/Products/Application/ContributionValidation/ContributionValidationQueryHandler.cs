using Common.SharedKernel.Domain;
using Products.Application.Abstractions;
using Products.Application.Abstractions.Rules;
using Products.Domain.Objectives;
using Products.Domain.Portfolios;
using Products.Integrations.ContributionValidation;

namespace Products.Application.ContributionValidation;

internal sealed class ContributionValidationQueryHandler(
    IObjectiveRepository objectiveRepository,
    IPortfolioRepository portfolioRepository,
    IRuleEvaluator<ProductsModuleMarker> ruleEvaluator)
{
    private const string RuleSetName = "Products.Contribution.Validation";

    public async Task<Result<ContributionValidationResponse>> Handle(
        ContributionValidationQuery request,
        CancellationToken cancellationToken)
    {
        var objective = await objectiveRepository
            .GetByIdAsync(request.ObjectiveId, cancellationToken);

        var objectiveExists = objective is not null;
        var alternativeId = objective?.AlternativeId;
        var objectiveBelongsToAffiliate = objectiveExists &&
                                          objective!.AffiliateId == request.ActivateId;

        var isPortfolioCodeProvided = !string.IsNullOrWhiteSpace(request.PortfolioStandardCode);

        var portfolioBelongsToAlternative = false;
        var collectorPortfolioFound = false;
        var effectivePortfolioCode = request.PortfolioStandardCode;

        if (isPortfolioCodeProvided)
        {
            portfolioBelongsToAlternative = objectiveExists &&
                                            await portfolioRepository.BelongsToAlternativeAsync(
                                                request.PortfolioStandardCode!,
                                                alternativeId!.Value,
                                                cancellationToken);
        }
        else if (objectiveExists)
        {
            effectivePortfolioCode = await portfolioRepository.GetCollectorCodeAsync(
                alternativeId!.Value, cancellationToken);

            collectorPortfolioFound = effectivePortfolioCode is not null;
        }

        Portfolio? portfolio = null;
        if (!string.IsNullOrWhiteSpace(effectivePortfolioCode))
            portfolio = await portfolioRepository.GetByStandardCodeAsync(
                effectivePortfolioCode!, cancellationToken);

        var validationContext = new
        {
            request.ObjectiveId,
            AlternativeId = alternativeId,
            ObjectiveBelongsToAffiliate = objectiveBelongsToAffiliate,
            IsPortfolioCodeProvided = isPortfolioCodeProvided,
            PortfolioBelongsToObjectiveAlternative = portfolioBelongsToAlternative,
            CollectorPortfolioFound = collectorPortfolioFound,
            PortfolioStandardCode = effectivePortfolioCode,
            request.DepositDate,
            request.ExecutionDate,
            request.Amount,
            ExistsByStandardCode = portfolio
        };

        var (isValid, _, errors) =
            await ruleEvaluator.EvaluateAsync(RuleSetName, validationContext, cancellationToken);

        if (!isValid)
        {
            var error = errors.First();
            return Result.Failure<ContributionValidationResponse>(
                Error.Validation(error.Code, error.Message));
        }

        return Result.Success(new ContributionValidationResponse(true, request.ActivateId, request.ObjectiveId,
            portfolio!.PortfolioId, portfolio.InitialMinimumAmount));
    }
}