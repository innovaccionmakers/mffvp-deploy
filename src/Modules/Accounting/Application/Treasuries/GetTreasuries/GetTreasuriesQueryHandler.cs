using Accounting.Domain.Treasuries;
using Accounting.Integrations.Treasuries.GetTreasuries;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.Treasuries.GetTreasuries
{
    internal class GetTreasuriesQueryHandler(
        ITreasuryRepository treasuryRepository,
        ILogger<GetTreasuriesQueryHandler> logger) : IQueryHandler<GetTreasuriesQuery, IReadOnlyCollection<GetTreasuriesResponse>>
    {
        public async Task<Result<IReadOnlyCollection<GetTreasuriesResponse>>> Handle(GetTreasuriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var treasury = await treasuryRepository.GetTreasuriesAsync(cancellationToken);

                if (treasury is null)
                    return Result.Failure<IReadOnlyCollection<GetTreasuriesResponse>>(Error.NotFound("0", "No se encontró registro de la tesorería"));

                var result = treasury.Select(t => new GetTreasuriesResponse(
                    t.TreasuryId,
                    t.BankAccount,
                    t.DebitAccount,
                    t.CreditAccount
                    )).ToList();

                return Result.Success<IReadOnlyCollection<GetTreasuriesResponse>>(result);
            }
            catch (Exception ex)
            {
                logger.LogError("Error al obtener tesorería. Error: {Message}", ex.Message);
                return Result.Failure<IReadOnlyCollection<GetTreasuriesResponse>>(Error.NotFound("0", "No hay registros de tesorería"));
            }
        }
    }
}
