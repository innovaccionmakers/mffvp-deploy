using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.Constants;
using Accounting.Domain.Consecutives;
using System.Linq;

namespace Accounting.Application.AccountingGeneration;

internal sealed class AccountingGenerationValidator
{
    public static string? ValidateNatureRecordLimits(IReadOnlyCollection<AccountingAssistant> accountingAssistants, IReadOnlyCollection<Consecutive> consecutives)
    {
        var consecutiveByNature = consecutives.ToDictionary(c => c.Nature, c => c.Number);

        var natureCounts = accountingAssistants
            .GroupBy(aa => aa.Nature)
            .Select(g => new
            {
                Nature = g.Key,
                Count = g.Count(),
                CurrentConsecutive = consecutiveByNature.GetValueOrDefault(g.Key, 0)
            })
            .ToList();

        var exceededNatures = natureCounts
            .Where(nc =>
            {
                // El último consecutivo sería: consecutivo actual + cantidad de registros - 1
                // Por ejemplo: si actual es 0 y hay 3 registros, mostrará 0, 1, 2 (último = 2)
                // Pero si actual es 0 y hay 3 registros, el último consecutivo mostrado será 2
                // Entonces: currentConsecutive + (count - 1) debe ser <= MaxConsecutiveNumber
                var lastConsecutive = nc.CurrentConsecutive + (nc.Count - 1);
                return lastConsecutive > AccountingReportConstants.MaxConsecutiveNumber;
            })
            .Select(nc =>
            {
                var lastConsecutive = nc.CurrentConsecutive + (nc.Count - 1);
                return $"{nc.Nature} (consecutivo actual: {nc.CurrentConsecutive}, último consecutivo: {lastConsecutive:N0}, máximo permitido: {AccountingReportConstants.MaxConsecutiveNumber:N0})";
            })
            .ToList();

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

