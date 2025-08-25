
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.YieldDetails;


namespace Closing.Application.PreClosing.Services.Yield;

public class YieldDetailCreationService : IYieldDetailCreationService
{

    private IYieldDetailRepository _yieldDetailRepository;
    public YieldDetailCreationService(
        IYieldDetailRepository yieldDetailRepository)
    {
        _yieldDetailRepository = yieldDetailRepository;
    }
    public async Task CreateYieldDetailsAsync(
    IEnumerable<YieldDetail> yieldDetails,
    PersistenceMode mode = PersistenceMode.Immediate,
    CancellationToken cancellationToken = default)
    {
        var list = yieldDetails as IReadOnlyList<YieldDetail> ?? yieldDetails.ToList();
        if (list.Count == 0) return;

        if (mode == PersistenceMode.Immediate)
        {
            // Simulación: contexto efímero, confirma adentro (paralelismo seguro)
            await _yieldDetailRepository.InsertRangeImmediateAsync(list, cancellationToken);
        }
        else
        {
            // Cierre: usa el contexto scoped; lo confirma UnitOfWork externo
            foreach (var d in list)
                await _yieldDetailRepository.InsertAsync(d, cancellationToken);
        }
    }

}