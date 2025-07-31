using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios.GetPortfolioById;
using Products.Integrations.Portfolios.Queries;

namespace Products.Application.Portfolios.GetPortfolioById;
internal sealed class GetPortfolioByIdHandler(
    IPortfolioRepository repository,
    IRuleEvaluator<ProductsModuleMarker> ruleEvaluator
    ) : IQueryHandler<GetPortfolioByIdQuery, GetPortfolioByIdResponse>
{

    public async Task<Result<GetPortfolioByIdResponse>> Handle(
        GetPortfolioByIdQuery query,
        CancellationToken cancellationToken)
    {
        //validar reglas de negocio
        var (requiredOk, _, requiredErrors) =
            await ruleEvaluator.EvaluateAsync(
            "Products.Portfolio.Required",
            new { query.CodigoPortafolio });

        if (!requiredOk)
        {
            var first = requiredErrors.First();
            return Result.Failure<GetPortfolioByIdResponse>(
                Error.Validation(first.Code, first.Message));
        }

        var portfolio = await repository.GetByHomologatedCodeAsync(
            query.CodigoPortafolio,
            cancellationToken);

        var (isValid, _, errors) = await ruleEvaluator
            .EvaluateAsync(
                "Products.PortfoliobyHomologatedCode.Validation",
                portfolio,
                cancellationToken);

        if (!isValid)
        {
            var first = errors.First();
            return Result.Failure<GetPortfolioByIdResponse>(
                Error.Validation(first.Code, first.Message));
        }

        return Result.Success(new GetPortfolioByIdResponse(
            portfolio.PortfolioId,
            portfolio.Name,
            portfolio.ShortName,
            portfolio.InitialMinimumAmount,
            portfolio.AdditionalMinimumAmount,
            portfolio.CurrentDate.AddDays(1)
        ));
    }
}