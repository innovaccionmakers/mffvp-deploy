namespace Activations.Application.Abstractions.Lookups
{
    public interface ILookupService
    {
        bool CodeExists(string table, string code);
        bool ValidatePensionerStatus(bool? pensioner);
        bool ValidatePensionRequirements(bool? pensioner, bool? meetsRequirements);
        bool ValidatePensionDates(bool? pensioner, bool? meetsRequirements, DateTime? startDate, DateTime? endDate);
    }
}
