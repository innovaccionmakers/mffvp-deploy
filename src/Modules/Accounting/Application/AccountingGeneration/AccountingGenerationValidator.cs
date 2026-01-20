using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.Constants;
using Accounting.Domain.Consecutives;
using System.Linq;

namespace Accounting.Application.AccountingGeneration;

internal sealed class AccountingGenerationValidator
{
    public static string? ValidateNatureRecordLimits(IReadOnlyCollection<AccountingAssistant> incomeAssistants,
                                                     IReadOnlyCollection<AccountingAssistant> egressAssistants,
                                                     IReadOnlyCollection<AccountingAssistant> yieldAssistants,
                                                     IReadOnlyCollection<AccountingAssistant> conceptsAssistants,
                                                     IReadOnlyCollection<Consecutive> consecutives)
    {
        var consecutiveByNature = consecutives.ToDictionary(c => c.Nature, c => c.Number);
        var exceededNatures = new List<string>();

        var uniqueIncomeCount = incomeAssistants.GroupBy(a => a.Identifier).Count();
        var uniqueEgressCount = egressAssistants.GroupBy(a => a.Identifier).Count();
        var uniqueYieldCount = yieldAssistants.GroupBy(a => a.Identifier).Count();
        var uniqueConceptCount = conceptsAssistants.GroupBy(a => a.Identifier).Count();

        if (uniqueIncomeCount > 0)
        {
            var incomeConsecutive = consecutiveByNature.GetValueOrDefault(NatureTypes.Income, 0);
            var lastIncomeConsecutive = incomeConsecutive + uniqueIncomeCount;

            if (lastIncomeConsecutive > AccountingReportConstants.MaxConsecutiveNumber)
            {
                exceededNatures.Add($"{NatureTypes.Income} (consecutivo actual: {incomeConsecutive}, último consecutivo: {lastIncomeConsecutive:N0}, máximo permitido: {AccountingReportConstants.MaxConsecutiveNumber:N0})");
            }
        }

        if (uniqueEgressCount > 0)
        {
            var egressConsecutive = consecutiveByNature.GetValueOrDefault(NatureTypes.Egress, 0);
            var lastEgressConsecutive = egressConsecutive + uniqueEgressCount;

            if (lastEgressConsecutive > AccountingReportConstants.MaxConsecutiveNumber)
            {
                exceededNatures.Add($"{NatureTypes.Egress} (consecutivo actual: {egressConsecutive}, último consecutivo: {lastEgressConsecutive:N0}, máximo permitido: {AccountingReportConstants.MaxConsecutiveNumber:N0})");
            }
        }

        if (uniqueYieldCount > 0)
        {
            var yieldConsecutive = consecutiveByNature.GetValueOrDefault(NatureTypes.Yields, 0);
            var lastYieldConsecutive = yieldConsecutive + uniqueYieldCount;

            if (lastYieldConsecutive > AccountingReportConstants.MaxConsecutiveNumber)
            {
                exceededNatures.Add($"{NatureTypes.Yields} (consecutivo actual: {yieldConsecutive}, último consecutivo: {lastYieldConsecutive:N0}, máximo permitido: {AccountingReportConstants.MaxConsecutiveNumber:N0})");
            }
        }

        if(uniqueConceptCount > 0)
        {
            var conceptConsecutive = consecutiveByNature.GetValueOrDefault(NatureTypes.Concept, 0);
            var lastConceptConsecutive = conceptConsecutive + uniqueConceptCount;
            if (lastConceptConsecutive > AccountingReportConstants.MaxConsecutiveNumber)
            {
                exceededNatures.Add($"{NatureTypes.Concept} (consecutivo actual: {conceptConsecutive}, último consecutivo: {lastConceptConsecutive:N0}, máximo permitido: {AccountingReportConstants.MaxConsecutiveNumber:N0})");
            }
        }

        if (exceededNatures.Count != 0)
        {
            return $"Se superó el límite máximo de consecutivos ({AccountingReportConstants.MaxConsecutiveNumber:N0}). " +
                $"Naturalezas excedidas: {string.Join("; ", exceededNatures)}";
        }

        return null;
    }

    public static string? ValidateConsecutivesExist(IReadOnlyCollection<Consecutive> consecutives)
    {
        var hasIncomeConsecutive = consecutives.Any(c => c.Nature == NatureTypes.Income);
        var hasEgressConsecutive = consecutives.Any(c => c.Nature == NatureTypes.Egress);

        var hasYieldConsecutive = consecutives.Any(c => c.Nature == NatureTypes.Yields);
        var hasConceptConsecutive = consecutives.Any(c => c.Nature == NatureTypes.Concept);

        if (!hasIncomeConsecutive || !hasEgressConsecutive || !hasYieldConsecutive || !hasConceptConsecutive)
        {
            var missingNatures = new List<string>();
            if (!hasIncomeConsecutive) missingNatures.Add(NatureTypes.Income);
            if (!hasEgressConsecutive) missingNatures.Add(NatureTypes.Egress);
            if (!hasYieldConsecutive) missingNatures.Add(NatureTypes.Yields);
            if (!hasConceptConsecutive) missingNatures.Add(NatureTypes.Concept);

            return $"No existen consecutivos configurados para las siguientes naturalezas: {string.Join(", ", missingNatures)}";
        }

        return null;
    }
}

