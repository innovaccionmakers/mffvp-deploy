using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Data;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.CreatePortfolio;

namespace Products.Application.Portfolios.CreatePortfolio;

internal sealed class CreatePortfolioCommandHandler(
    IPortfolioRepository portfolioRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreatePortfolioCommand, PortfolioResponse>
{
    public async Task<Result<PortfolioResponse>> Handle(CreatePortfolioCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);


        var result = Portfolio.Create(
            request.StandardCode,
            request.Name,
            request.ShortName,
            request.ModalityId,
            request.InitialMinimumAmount
        );

        if (result.IsFailure) return Result.Failure<PortfolioResponse>(result.Error);

        var portfolio = result.Value;

        portfolioRepository.Insert(portfolio);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new PortfolioResponse(
            portfolio.PortfolioId,
            portfolio.StandardCode,
            portfolio.Name,
            portfolio.ShortName,
            portfolio.ModalityId,
            portfolio.InitialMinimumAmount
        );
    }
}