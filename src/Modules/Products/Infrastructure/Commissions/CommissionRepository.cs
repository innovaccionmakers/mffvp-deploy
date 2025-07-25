﻿using Common.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Products.Domain.Commissions;
using Products.Infrastructure.Database;

namespace Products.Infrastructure.Commissions
{
    internal sealed class CommissionRepository(ProductsDbContext context) : ICommissionRepository
    {
        public async Task<IReadOnlyCollection<Commission>> GetActiveCommissionsByPortfolioAsync (int portfolioId, CancellationToken cancellationToken = default)
        {
           return await context.Commissions.Where(x => x.PortfolioId == portfolioId && x.Status == Status.Active).ToListAsync(cancellationToken);
        }

    }
}
