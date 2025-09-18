using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Products.Application.Abstractions;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.Queries;

namespace Products.Application.Portfolios.QueriesHandlers;

internal sealed class GetPortfolioInformationByIdQueryHandler(IPortfolioRepository portfolioRepository,
                                                              IRuleEvaluator<ProductsModuleMarker> ruleEvaluator,
                                                              ILogger<GetPortfolioInformationByIdQueryHandler> logger) : IQueryHandler<GetPortfolioInformationByIdQuery, CompletePortfolioInformationResponse>
{
    private const string ValidationWorkflow = "Products.Portfolio.Validation";

    public async Task<Result<CompletePortfolioInformationResponse>> Handle(GetPortfolioInformationByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var portfolio = await portfolioRepository.GetAsync(request.PortfolioId, cancellationToken);

            var (isValid, _, errors) = await ruleEvaluator
               .EvaluateAsync(
                   ValidationWorkflow,
                   portfolio,
                   cancellationToken);

            if (!isValid)
            {
                var first = errors.First();
                return Result.Failure<CompletePortfolioInformationResponse>(
                    Error.Validation(first.Code, first.Message));
            }

            if (portfolio is null)
            {
                logger.LogWarning("No se encontró el portafolio con ID {PortfolioId}.", request.PortfolioId);
                return Result.Failure<CompletePortfolioInformationResponse>(new Error("Error", "No se encontró el portafolio con el ID proporcionado.", ErrorType.Validation));
            }

            return Result.Success(new CompletePortfolioInformationResponse(
                PortfolioId: portfolio.PortfolioId,
                Name: portfolio.Name,
                ShortName: portfolio.ShortName,
                ModalityId: portfolio.ModalityId,
                InitialMinimumAmount: portfolio.InitialMinimumAmount,
                AdditionalMinimumAmount: portfolio.AdditionalMinimumAmount,
                CurrentDate: portfolio.CurrentDate,
                HomologatedCode: portfolio.HomologatedCode,
                VerificationDigit: portfolio.VerificationDigit,
                PortfolioNIT: portfolio.PortfolioNIT,
                NitApprovedPortfolio: portfolio.NitApprovedPortfolio,
                RiskProfile: portfolio.RiskProfile,
                SFCBusinessCode: portfolio.SFCBusinessCode,
                Custodian: portfolio.Custodian,
                Qualifier: portfolio.Qualifier,
                Rating: portfolio.Rating,
                RatingType: portfolio.RatingType,
                LastRatingDate: portfolio.LastRatingDate,
                AdviceClassification: portfolio.AdviceClassification,
                MaxParticipationPercentage: portfolio.MaxParticipationPercentage,
                MinimumVirPercentage: portfolio.MinimumVirPercentage,
                PartialVirPercentage: portfolio.PartialVirPercentage,
                AgileWithdrawalPercentageProtectedBalance: portfolio.AgileWithdrawalPercentageProtectedBalance,
                WithdrawalPercentageProtectedBalance: portfolio.WithdrawalPercentageProtectedBalance,
                AllowsAgileWithdrawal: portfolio.AllowsAgileWithdrawal,
                PermanencePeriod: portfolio.PermanencePeriod,
                PenaltyPercentage: portfolio.PenaltyPercentage,
                OperationsStartDate: portfolio.OperationsStartDate,
                PortfolioExpiryDate: portfolio.PortfolioExpiryDate,
                IndustryClassification: portfolio.IndustryClassification,
                Status: portfolio.Status.ToString()
            ));
        }
        catch (Exception ex)
        { 
            logger.LogError(ex, "Ocurrió un error inesperado al obtener la información del portafolio con ID {PortfolioId}.", request.PortfolioId);
            return Result.Failure<CompletePortfolioInformationResponse>(new Error("Error", "Ocurrió un error inesperado al obtener la información del portafolio.", ErrorType.Problem));
        }
    }
}
