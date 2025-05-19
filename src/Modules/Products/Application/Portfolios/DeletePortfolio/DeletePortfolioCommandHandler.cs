using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Data;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios.DeletePortfolio;

namespace Products.Application.Portfolios.DeletePortfolio;

internal sealed class DeletePortfolioCommandHandler(
    IPortfolioRepository portfolioRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeletePortfolioCommand>
{
    public async Task<Result> Handle(DeletePortfolioCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var portfolio = await portfolioRepository.GetAsync(request.PortfolioId, cancellationToken);
        if (portfolio is null) return Result.Failure(PortfolioErrors.NotFound(request.PortfolioId));

        portfolioRepository.Delete(portfolio);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}