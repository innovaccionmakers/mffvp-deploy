using Closing.Domain.ProfitLossConcepts;
using Common.SharedKernel.Domain;

namespace Closing.Domain.ProfitLosses;

public sealed record ProfitLossSummary(string Concept, IncomeExpenseNature Nature, decimal TotalAmount);