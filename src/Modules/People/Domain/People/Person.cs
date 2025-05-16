using Common.SharedKernel.Domain;
using People.Domain.Countries;
using People.Domain.EconomicActivities;

namespace People.Domain.People;
public sealed class Person : Entity
{
    public long PersonId { get; private set; }
    public string DocumentType { get; private set; }
    public string StandardCode { get; private set; }
    public string Identification { get; private set; }
    public string FirstName { get; private set; }
    public string MiddleName { get; private set; }
    public string LastName { get; private set; }
    public string SecondLastName { get; private set; }
    public DateTime IssueDate { get; private set; }
    public int IssueCityId { get; private set; }
    public DateTime BirthDate { get; private set; }
    public int BirthCityId { get; private set; }
    public string Mobile { get; private set; }
    public string FullName { get; private set; }
    public int MaritalStatusId { get; private set; }
    public int GenderId { get; private set; }
    public int CountryId { get; private set; }
    public string Email { get; private set; }
    public string EconomicActivityId { get; private set; }

    private Person() { }

    public static Result<Person> Create(
        string documentType, string standardCode, string identification, string firstName, string middleName, string lastName, string secondLastName, DateTime issueDate, int issueCityId, DateTime birthDate, int birthCityId, string mobile, string fullName, int maritalStatusId, int genderId, string email, Country country, EconomicActivity economicActivity
    )
    {
        var person = new Person
        {
            PersonId = default(long),

            DocumentType = documentType,
            StandardCode = standardCode,
            Identification = identification,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            SecondLastName = secondLastName,
            IssueDate = issueDate,
            IssueCityId = issueCityId,
            BirthDate = birthDate,
            BirthCityId = birthCityId,
            Mobile = mobile,
            FullName = fullName,
            MaritalStatusId = maritalStatusId,
            GenderId = genderId,
            CountryId = country.CountryId,
            Email = email,
            EconomicActivityId = economicActivity.EconomicActivityId,
        };

        person.Raise(new PersonCreatedDomainEvent(person.PersonId));
        return Result.Success(person);
    }

        public void UpdateDetails(
        string newDocumentType, string newStandardCode, string newIdentification, string newFirstName, string newMiddleName, string newLastName, string newSecondLastName, DateTime newIssueDate, int newIssueCityId, DateTime newBirthDate, int newBirthCityId, string newMobile, string newFullName, int newMaritalStatusId, int newGenderId, int newCountryId, string newEmail, string newEconomicActivityId
    )
    {
        DocumentType = newDocumentType;
        StandardCode = newStandardCode;
        Identification = newIdentification;
        FirstName = newFirstName;
        MiddleName = newMiddleName;
        LastName = newLastName;
        SecondLastName = newSecondLastName;
        IssueDate = newIssueDate;
        IssueCityId = newIssueCityId;
        BirthDate = newBirthDate;
        BirthCityId = newBirthCityId;
        Mobile = newMobile;
        FullName = newFullName;
        MaritalStatusId = newMaritalStatusId;
        GenderId = newGenderId;
        CountryId = newCountryId;
        Email = newEmail;
        EconomicActivityId = newEconomicActivityId;
    }

}