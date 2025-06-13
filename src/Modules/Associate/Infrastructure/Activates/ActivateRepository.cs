using Amazon.Runtime.Internal.Util;

using Associate.Domain.Activates;
using Associate.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Associate.Infrastructure;

public class ActivateRepository : IActivateRepository
{
    private readonly AssociateDbContext _context;
    private readonly ILogger<ActivateRepository> _logger;

    public ActivateRepository(AssociateDbContext context, ILogger<ActivateRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<Activate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(_context.Database.GetDbConnection().ConnectionString);
        return await _context.Activates.ToListAsync(cancellationToken);
    }

    public void Insert(Activate activate, CancellationToken cancellationToken = default)
    {
        _context.Activates.Add(activate);
    }    

    public void Update(Activate activate, CancellationToken cancellationToken = default)
    {
        _context.Activates.Update(activate);
    }

    public async Task<Activate?> GetByIdTypeAndNumber(Guid IdentificationType, string identification, CancellationToken cancellationToken = default)
    {
        return await _context.Activates.SingleOrDefaultAsync(a =>
            a.IdentificationType == IdentificationType && a.Identification == identification);
    }

    public async Task<Activate?> GetByIdAsync(long activateId, CancellationToken cancellationToken = default)
    {
        return await _context.Activates.SingleOrDefaultAsync(x => x.ActivateId == activateId, cancellationToken);
    }
}