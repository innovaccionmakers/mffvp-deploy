using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Products.Domain.PensionFunds;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.PensionFunds
{
    internal sealed class PensionFundRepository : IPensionFundRepository
    {
        #region vars
        private readonly ProductsDbContext _context;
        private readonly ILogger<PensionFundRepository> _logger;
        #endregion

        public PensionFundRepository(ProductsDbContext context, ILogger<PensionFundRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<PensionFund>> GetAllPensionFundsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(_context.Database.GetDbConnection().ConnectionString);
            return await _context.PensionFunds.ToListAsync(cancellationToken);
        }
    }
}
