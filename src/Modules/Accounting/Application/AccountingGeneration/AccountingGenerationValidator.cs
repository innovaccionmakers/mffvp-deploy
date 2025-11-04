using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.Constants;
using Accounting.Domain.Consecutives;
using System.Linq;

namespace Accounting.Application.AccountingGeneration;

internal sealed class AccountingGenerationValidator
{
    public static string? ValidateNatureRecordLimits(IReadOnlyCollection<AccountingAssistant> accountingAssistants)
    {
        var natureCounts = accountingAssistants
            .GroupBy(aa => aa.Nature == NatureTypes.Expense ? NatureTypes.Egress : NatureTypes.Income)
            .Select(g => new { Nature = g.Key, Count = g.Count() })
            .ToList();

        var exceededNatures = natureCounts
            .Where(nc => nc.Count > AccountingReportConstants.MaxConsecutiveNumber)
            .Select(nc => $"{nc.Nature} ({nc.Count:N0} registros)")
            .ToList();

        if (exceededNatures.Count != 0)
        {
            return $"Se superó el límite máximo de {AccountingReportConstants.MaxConsecutiveNumber:N0} registros por naturaleza. " +
                $"Naturalezas excedidas: {string.Join(", ", exceededNatures)}";
        }

        return null;
    }

    public static string? ValidateConsecutivesExist(IReadOnlyCollection<Consecutive> consecutives)
    {
        var hasIncomeConsecutive = consecutives.Any(c => c.Nature == NatureTypes.Income);
        var hasEgressConsecutive = consecutives.Any(c => c.Nature == NatureTypes.Egress);

        if (!hasIncomeConsecutive || !hasEgressConsecutive)
        {
            var missingNatures = new List<string>();
            if (!hasIncomeConsecutive) missingNatures.Add(NatureTypes.Income);
            if (!hasEgressConsecutive) missingNatures.Add(NatureTypes.Egress);

            return $"No existen consecutivos configurados para las siguientes naturalezas: {string.Join(", ", missingNatures)}";
        }

        return null;
    }
}

