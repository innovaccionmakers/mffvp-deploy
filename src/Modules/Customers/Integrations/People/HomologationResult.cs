namespace Integrations.People
{
    public class HomologationResult
    {
        public int? AdditionalId { get; set; }
        public Guid? Uuid { get; set; } 

         // Propiedades de homologación
        public bool DocumentTypeHomologated { get; set; }
        public bool CountryHomologated { get; set; }
        public bool DepartmentHomologated { get; set; }
        public bool MunicipalityHomologated { get; set; }
        public bool EconomicActivityHomologated { get; set; }
        public bool GenderHomologated { get; set; }
        public bool InvestorTypeHomologated { get; set; }
        public bool RiskProfileHomologated { get; set; }

        // IDs para creación
        public int? CountryId { get; set; }
        public int? DepartmentId { get; set; }
        public int? MunicipalityId { get; set; }
        public int? EconomicActivityId { get; set; }
        public int? GenderId { get; set; }
        public int? InvestorTypeId { get; set; }
        public int? RiskProfileId { get; set; }
    }
}