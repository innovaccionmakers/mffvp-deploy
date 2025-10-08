using Closing.Domain.PortfolioValuations;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Products.Application.Abstractions;
using Products.Application.Abstractions.Data;
using Products.Application.Abstractions.External.Closing;
using Products.Application.Abstractions.External.Trusts;
using Products.Domain.TechnicalSheets;
using Products.Integrations.TechnicalSheets.Commands;

namespace Products.Application.TechnicalSheets.Commands;

internal sealed class SaveTechnicalSheetCommandHandler(
    ITechnicalSheetRepository technicalSheetRepository,
    IUnitOfWork unitOfWork,
    IPortfolioValuationLocator portfolioValuationLocator,
    ITrustYieldLocator trustYieldLocator,
    IInternalRuleEvaluator<ProductsModuleMarker> ruleEvaluator,
    ILogger<SaveTechnicalSheetCommandHandler> logger)
    : ICommandHandler<SaveTechnicalSheetCommand, Unit>
{
    private const string TechnicalSheetValidationWorkflow = "Products.TechnicalSheets.Validation";
    public async Task<Result<Unit>> Handle(SaveTechnicalSheetCommand request, CancellationToken cancellationToken)
    {
        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var closingDate = request.ClosingDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

            var existsTechnicalSheets = await technicalSheetRepository.ExistsByDateAsync(closingDate, cancellationToken);

            if (existsTechnicalSheets)
                await technicalSheetRepository.DeleteByDateAsync(closingDate, cancellationToken);

            var portfolioValuationsResponse = await portfolioValuationLocator.GetPortfolioValuationAsync(closingDate, cancellationToken);

            var portfolioValuations = portfolioValuationsResponse.Value ?? [];

            var validationContext = new
            {
                PortfolioValuationsExits = portfolioValuations.Count > 0,
            };


            var (rulesOk, _, ruleErrors) = await ruleEvaluator
                .EvaluateAsync(TechnicalSheetValidationWorkflow,
                    validationContext,
                    cancellationToken);

            if (!rulesOk)
            {
                var first = ruleErrors.First();
                return Result.Failure<Unit>(
                    Error.Validation(first.Code, first.Message));
            }

            var technicalSheets = await CreateRange(portfolioValuations, closingDate, cancellationToken);

            if (technicalSheets.Any())
            {
                await technicalSheetRepository.AddRangeAsync(technicalSheets, cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
            return Result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error en SaveTechnicalSheetCommandHandler para ClosingDate {ClosingDate}", request.ClosingDate);
            throw;
        }
    }

    private async Task<IEnumerable<TechnicalSheet>> CreateRange(
        IEnumerable<PortfolioValuationResponse> portfolioValuations,
        DateTime closingDate,
        CancellationToken cancellationToken)
    {
        var technicalSheets = new List<TechnicalSheet>();

        foreach (var portfolioValuation in portfolioValuations)
        {
            var participants = (portfolioValuation.TrustIds?.Any() == true)
            ? (await trustYieldLocator.GetParticipant(portfolioValuation.TrustIds, cancellationToken))
                .Match(value => value, _ => 0)
            : 0;

            var technicalSheet = TechnicalSheet.Create(
                portfolioValuation.PortfolioId,
                closingDate,
                portfolioValuation.Contributions,
                portfolioValuation.Withdrawals,
                portfolioValuation.PygBruto,
                portfolioValuation.Expenses,
                portfolioValuation.CommissionDay,
                portfolioValuation.CostDay,
                portfolioValuation.YieldToCredit,
                portfolioValuation.GrossYieldPerUnit,
                portfolioValuation.CostPerUnit,
                portfolioValuation.UnitValue,
                portfolioValuation.Units,
                portfolioValuation.AmountPortfolio,
                participants
            );
            if(technicalSheet.IsSuccess)
                technicalSheets.Add(technicalSheet.Value);
        }

        return technicalSheets;
    }
}