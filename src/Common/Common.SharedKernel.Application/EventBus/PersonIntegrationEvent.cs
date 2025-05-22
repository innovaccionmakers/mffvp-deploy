namespace Common.SharedKernel.Application.EventBus
{
    public sealed record PersonDataRequestEvent(
        string DocumentType,
        string Identification
        ) : IPersonIntegrationEvent;

    public sealed record PersonDataResponseEvent(
        long PersonId,
        string DocumentType,
        string StandardCode,
        string Identification,
        string FirstName,
        string MiddleName,
        string LastName,
        string SecondLastName,
        DateTime IssueDate,
        int IssueCityId,
        DateTime BirthDate,
        int BirthCityId,
        string Mobile,
        string FullName,
        int MaritalStatusId,
        int GenderId,
        int CountryId,
        string Email,
        string EconomicActivityId) : IPersonIntegrationEvent;
}