using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.Constants;
using Accounting.Domain.Consecutives;
using System.Linq;

namespace Accounting.Application.AccountingGeneration;

internal sealed class AccountingGenerationValidator
{
    public static string? ValidateNatureRecordLimits(IReadOnlyCollection<AccountingAssistant> incomeAssistants,
                                                     IReadOnlyCollection<AccountingAssistant> egressAssistants,
                                                     IReadOnlyCollection<Consecutive> consecutives)
    {
        var consecutiveByNature = consecutives.ToDictionary(c => c.Nature, c => c.Number);
        var exceededNatures = new List<string>();

        // Contar grupos únicos por Identifier (los duplicados comparten consecutivo)
        var uniqueIncomeCount = incomeAssistants.GroupBy(a => a.Identifier).Count();
        var uniqueEgressCount = egressAssistants.GroupBy(a => a.Identifier).Count();

        if (uniqueIncomeCount > 0)
        {
            var incomeConsecutive = consecutiveByNature.GetValueOrDefault(NatureTypes.Income, 0);
            var lastIncomeConsecutive = incomeConsecutive + (uniqueIncomeCount - 1);

            if (lastIncomeConsecutive > AccountingReportConstants.MaxConsecutiveNumber)
            {
                exceededNatures.Add($"{NatureTypes.Income} (consecutivo actual: {incomeConsecutive}, último consecutivo: {lastIncomeConsecutive:N0}, máximo permitido: {AccountingReportConstants.MaxConsecutiveNumber:N0})");
            }
        }

        if (uniqueEgressCount > 0)
        {
            var egressConsecutive = consecutiveByNature.GetValueOrDefault(NatureTypes.Egress, 0);
            var lastEgressConsecutive = egressConsecutive + (uniqueEgressCount - 1);

            if (lastEgressConsecutive > AccountingReportConstants.MaxConsecutiveNumber)
            {
                exceededNatures.Add($"{NatureTypes.Egress} (consecutivo actual: {egressConsecutive}, último consecutivo: {lastEgressConsecutive:N0}, máximo permitido: {AccountingReportConstants.MaxConsecutiveNumber:N0})");
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

