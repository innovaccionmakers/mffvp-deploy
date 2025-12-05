using Accounting.Domain.Treasuries;
using Accounting.Integrations.Treasury.GetTreasuries;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.Treasury.GetTreasuries
{
    internal class GetTreasuryQueryHandler(
        ITreasuryRepository treasuryRepository,
        ILogger<GetTreasuryQueryHandler> logger) : IQueryHandler<GetTreasuryQuery, GetTreasuryResponse>
    {
        public async Task<Result<GetTreasuryResponse>> Handle(GetTreasuryQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var treasury = await treasuryRepository.GetTreasuryAsync(request.PortfolioId, request.BankAccount, cancellationToken);

                if (treasury is null)
                    return Result.Failure<GetTreasuryResponse>(Error.NotFound("0", "No se encontró registro de la tesorería"));

                var result = new GetTreasuryResponse(
                    treasury.TreasuryId,
                    treasury.BankAccount,
                    treasury.DebitAccount,
                    treasury.CreditAccount
                    );

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                logger.LogError("Error al obtener tesorería. Error: {Message}", ex.Message);
                return Result.Failure<GetTreasuryResponse>(Error.NotFound("0", "No hay registros de tesorería"));
            }
        }
    }
}
