using Closing.Application.Abstractions.External;
using Closing.Domain.ProfitLossConcepts;
using Closing.Domain.ProfitLosses;
using Closing.Integrations.ProfitLosses.GetProfitandLoss;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Closing.Application.ProfitLosses.GetProfitandLoss;

internal sealed class GetProfitandLossQueryHandler(
    IProfitLossRepository profitLossRepository,
    IPortfolioValidator portfolioValidator
) : IQueryHandler<GetProfitandLossQuery, GetProfitandLossResponse>
{
    public async Task<Result<GetProfitandLossResponse>> Handle(
        GetProfitandLossQuery request,
        CancellationToken cancellationToken)
    {
        var validationResult = await portfolioValidator
            .EnsureExistsAsync(request.PortfolioId, cancellationToken);
        if (!validationResult.IsSuccess)
            return Result.Failure<GetProfitandLossResponse>(validationResult.Error!);

        var summaries = await profitLossRepository
            .GetSummaryAsync(request.PortfolioId, request.EffectiveDate, cancellationToken);

        var values = summaries.ToDictionary(x => x.Concept, x => x.TotalAmount);
        var incomes = summaries
            .Where(x => x.Nature == IncomeExpenseNature.Income)
            .Sum(x => x.TotalAmount);
        var expenses = summaries
            .Where(x => x.Nature == IncomeExpenseNature.Expense)
            .Sum(x => x.TotalAmount);
        decimal netReturn = incomes - expenses;

        var response = new GetProfitandLossResponse(values, netReturn);
        return Result.Success(response,"Consulta realizada correctamente");
    }
}