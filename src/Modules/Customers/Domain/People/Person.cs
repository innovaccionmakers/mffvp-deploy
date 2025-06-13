using Common.SharedKernel.Domain;

namespace Customers.Domain.People;

public sealed class Person : Entity
{
    public long PersonId { get; private set; }
    public Guid IdentificationType { get; private set; }
    public string HomologatedCode { get; private set; }
    public string Identification { get; private set; }
    public string FirstName { get; private set; }
    public string? MiddleName { get; private set; }
    public string LastName { get; private set; }
    public string? SecondLastName { get; private set; }
    public DateTime BirthDate { get; private set; }
    public string FullName { get; private set; }
    public string Mobile { get; private set; }
    public int GenderId { get; private set; }
    public int CountryOfResidenceId { get; private set; }
    public int DepartmentId { get; private set; }
    public int MunicipalityId { get; private set; }
    public string Email { get; private set; }
    public int EconomicActivityId { get; private set; }
    public string Status { get; private set; }
    public string Address { get; private set; }
    public bool IsDeclarant { get; private set; }
    public int InvestorTypeId { get; private set; }
    public int RiskProfileId { get; private set; }

    private Person()
    {
    }

    public static Result<Person> Create(
        string? homologatedCode,
        string identification,
        string firstName,
        string? middleName,
        string lastName,
        string? secondLastName,        
        DateTime birthDate,
        string mobile,
        int genderId,
        int countryOfResidenceId,
        int departmentId,
        int municipalityId,
        string email,
        int economicActivityId,
        string address,
        bool isDeclarant,
        int investorTypeId,
        int riskProfileId)
    {
        var person = new Person
        {
            PersonId = default,
            IdentificationType = new Guid(),
            HomologatedCode = homologatedCode ?? string.Empty,
            Identification = identification,
            FirstName = firstName,
            MiddleName = middleName ?? string.Empty,
            LastName = lastName,
            SecondLastName = secondLastName ?? string.Empty,
            BirthDate = birthDate,
            Mobile = mobile,
            FullName = $"{firstName} {middleName} {lastName} {secondLastName}",
            GenderId = genderId,
            CountryOfResidenceId = countryOfResidenceId,
            DepartmentId = departmentId,
            MunicipalityId = municipalityId,
            Email = email,
            EconomicActivityId = economicActivityId,
            Status = "Activo",
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
        string newStatus,
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