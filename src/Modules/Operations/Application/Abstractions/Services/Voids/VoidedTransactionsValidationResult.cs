using System;
using System.Collections.Generic;
using Operations.Domain.ClientOperations;
using Operations.Integrations.Voids.RegisterVoidedTransactions;

namespace Operations.Application.Abstractions.Services.Voids;

public sealed record VoidedTransactionsValidationResult(
    IReadOnlyCollection<VoidedTransactionReady> ValidOperations,
    IReadOnlyCollection<VoidedTransactionFailure> FailedOperations,
    int CauseConfigurationParameterId);

public sealed record VoidedTransactionReady(
    ClientOperation OriginalOperation,
    decimal Amount,
    DateTime PortfolioCurrentDate,
    long TrustId);
