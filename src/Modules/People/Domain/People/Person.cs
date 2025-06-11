using Common.SharedKernel.Domain;

namespace People.Domain.People;

public sealed class Person : Entity
{
    public long PersonId { get; private set; }
    public Guid IdentificationType { get; private set; }
    public string HomologatedCode { get; private set; }
    public string Identification { get; private set; }
    public string FirstName { get; private set; }
    public string MiddleName { get; private set; }
    public string LastName { get; private set; }
    public string SecondLastName { get; private set; }
    public string Mobile { get; private set; }
    public string FullName { get; private set; }
    public int GenderId { get; private set; }
    public int CountryOfResidenceId { get; private set; }
    public int DepartmentId { get; private set; }
    public int MunicipalityId { get; private set; }
    public string Email { get; private set; }
    public int EconomicActivityId { get; private set; }
    public bool Status { get; private set; }
    public string Address { get; private set; }
    public bool IsDeclarant { get; private set; }
    public int InvestorTypeId { get; private set; }
    public int RiskProfileId { get; private set; }

    private Person()
    {
    }

    public static Result<Person> Create(
        Guid identificationType,
        string homologatedCode,
        string identification,
        string firstName,
        string middleName,
        string lastName,
        string secondLastName,
        string mobile,
        string fullName,
        int genderId,
        int countryOfResidenceId,
        int departmentId,
        int municipalityId,
        string email,
        int economicActivityId,
        bool status,
        string address,
        bool isDeclarant,
        int investorTypeId,
        int riskProfileId)
    {
        var person = new Person
        {
            PersonId = default,
            IdentificationType = identificationType,
            HomologatedCode = homologatedCode,
            Identification = identification,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            SecondLastName = secondLastName,
            Mobile = mobile,
            FullName = fullName,
            GenderId = genderId,
            CountryOfResidenceId = countryOfResidenceId,
            DepartmentId = departmentId,
            MunicipalityId = municipalityId,
            Email = email,
            EconomicActivityId = economicActivityId,
            Status = status,
            Address = address,
            IsDeclarant = isDeclarant,
            InvestorTypeId = investorTypeId,
            RiskProfileId = riskProfileId
        };

        person.Raise(new PersonCreatedDomainEvent(person.PersonId));
        return Result.Success(person);
    }

    public void UpdateDetails(
        Guid newIdentificationType,
        string newHomologatedCode,
        string newIdentification,
        string newFirstName,
        string newMiddleName,
        string newLastName,
        string newSecondLastName,
        string newMobile,
        string newFullName,
        int newGenderId,
        int newCountryOfResidenceId,
        int newDepartmentId,
        int newMunicipalityId,
        string newEmail,
        int newEconomicActivityId,
        bool newStatus,
        string newAddress,
        bool newIsDeclarant,
        int newInvestorTypeId,
        int newRiskProfileId)
    {
        IdentificationType = newIdentificationType;
        HomologatedCode = newHomologatedCode;
        Identification = newIdentification;
        FirstName = newFirstName;
        MiddleName = newMiddleName;
        LastName = newLastName;
        SecondLastName = newSecondLastName;
        Mobile = newMobile;
        FullName = newFullName;
        GenderId = newGenderId;
        CountryOfResidenceId = newCountryOfResidenceId;
        DepartmentId = newDepartmentId;
        MunicipalityId = newMunicipalityId;
        Email = newEmail;
        EconomicActivityId = newEconomicActivityId;
        Status = newStatus;
        Address = newAddress;
        IsDeclarant = newIsDeclarant;
        InvestorTypeId = newInvestorTypeId;
        RiskProfileId = newRiskProfileId;
    }
}