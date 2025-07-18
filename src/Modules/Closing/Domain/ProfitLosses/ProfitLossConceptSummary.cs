using Common.SharedKernel.Domain;

namespace Closing.Domain.ProfitLosses;

public sealed record ProfitLossConceptSummary(
    long ConceptId, 
    string ConceptName, 
    IncomeExpenseNature Nature,
    string Source, 
    decimal TotalAmount
 );

