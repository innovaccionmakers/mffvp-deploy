﻿using Common.SharedKernel.Domain;
namespace Treasury.Application.Abstractions.External;

public interface IPortfolioLocator
{
    Task<Result<(long PortfolioId, string Name, DateTime CurrentDate)>> FindByPortfolioIdAsync(int PortfolioId, CancellationToken ct);
}
