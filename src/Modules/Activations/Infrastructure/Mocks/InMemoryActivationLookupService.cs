using System.Collections.Concurrent;
using Activations.Application.Abstractions.Lookups;

namespace Activations.Infrastructure.Mocks;

internal sealed class InMemoryActivationLookupService : ILookupService
{
    private readonly ConcurrentDictionary<(string Table, string Code), bool> _data = new()
    {
        [("IdentificationType", "CC")] = true,
        [("IdentificationType", "TI")] = false,
        [("IdentificationType", "PP")] = false,
        [("IdentificationType", "CE")] = true
    };

    public bool CodeExists(string table, string code)
    {
        return _data.ContainsKey((table, code));
    }

    public bool ValidatePensionerStatus(bool? pensioner)
    {
        // Valores válidos: true o false
        return pensioner.HasValue;
    }

    public bool ValidatePensionRequirements(bool? pensioner, bool? meetsRequirements)
    {
        // Si es pensionado, no necesita requisitos
        if (pensioner == true) return true;

        // Si no es pensionado, debe tener valor en meetsRequirements
        return meetsRequirements.HasValue;
    }

    public bool ValidatePensionDates(bool? pensioner, bool? meetsRequirements, DateTime? startDate, DateTime? endDate)
    {
        // Solo valida fechas si pensioner == false y meetsRequirements == true
        if (pensioner == false && meetsRequirements == true) return startDate.HasValue && endDate.HasValue;
        return true;
    }
}