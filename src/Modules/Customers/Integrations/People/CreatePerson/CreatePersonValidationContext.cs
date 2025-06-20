using Customers.Domain.People;
using Integrations.People;
using Integrations.People.CreatePerson;

namespace Application.People.CreatePerson
{
    public class CreatePersonValidationContext
    {
        public CreatePersonRequestCommand Request { get; }
        public Person? ExistingPerson { get; }
        public Guid Uuid { get; }

        public bool? ExistingHomologatedCode { get; set; }
        public bool DocumentTypeHomologated { get; set; }
        public bool CountryHomologated { get; set; }
        public bool DepartmentHomologated { get; set; }
        public bool MunicipalityHomologated { get; set; }
        public bool EconomicActivityHomologated { get; set; }
        public bool GenderHomologated { get; set; }
        public bool InvestorTypeHomologated { get; set; }
        public bool RiskProfileHomologated { get; set; }
        public int DocumentTypeId { get; set; }
        public int? CountryId { get; set; }
        public int? DepartmentId { get; set; }
        public int? MunicipalityId { get; set; }
        public int? EconomicActivityId { get; set; }
        public int? GenderId { get; set; }
        public int? InvestorTypeId { get; set; }
        public int? RiskProfileId { get; set; }

        public CreatePersonValidationContext(CreatePersonRequestCommand request, Person? existingPerson, Guid uuid)
        {
            Request = request;
            ExistingPerson = existingPerson;
            Uuid = uuid;
        }
    }
}