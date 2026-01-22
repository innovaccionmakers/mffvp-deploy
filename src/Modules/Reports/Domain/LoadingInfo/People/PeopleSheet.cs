using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Reports.Domain.LoadingInfo.People;

public sealed class PeopleSheet : Entity
{
    public long Id { get; set; }
    public required string IdentificationType { get; set; }
    public required string NormalizedIdentificationType { get; set; }
    public required long AffiliateId { get; set; }
    public required string IdentificationNumber { get; set; }
    public required string FullName { get; set; }
    public DateTime? BirthDate { get; set; }
    public required string Gender { get; set; }

    public static Result<PeopleSheet> Create(
        string identificationType,
        string normalizedIdentificationType,
        long affiliateId,
        string identificationNumber,
        string fullName,
        DateTime? birthDate,
        string gender)
    {
        if (string.IsNullOrWhiteSpace(identificationType))
        {
            return Result.Failure<PeopleSheet>(
         new Error("PEOPLE_SHEET_001", "IdentificationType es requerido.", ErrorType.Validation));
        }

        if (string.IsNullOrWhiteSpace(normalizedIdentificationType))
        {
            return Result.Failure<PeopleSheet>(
                 new Error("PEOPLE_SHEET_002", "NormalizedIdentificationType es requerido.", ErrorType.Validation));
        }

        if (affiliateId <= 0)
        {
            return Result.Failure<PeopleSheet>(
                 new Error("PEOPLE_SHEET_003", "AffiliateId debe ser mayor a 0.", ErrorType.Validation));
        }

        if (string.IsNullOrWhiteSpace(identificationNumber))
        {
            return Result.Failure<PeopleSheet>(
                  new Error("PEOPLE_SHEET_004", "IdentificationNumber es requerido.", ErrorType.Validation));
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Result.Failure<PeopleSheet>(
         new Error("PEOPLE_SHEET_005", "FullName es requerido.", ErrorType.Validation));
        }

        if (string.IsNullOrWhiteSpace(gender))
        {
            return Result.Failure<PeopleSheet>(
            new Error("PEOPLE_SHEET_006", "Gender es requerido.", ErrorType.Validation));
        }

        var peopleSheet = new PeopleSheet
        {
            IdentificationType = identificationType,
            NormalizedIdentificationType = normalizedIdentificationType,
            AffiliateId = affiliateId,
            IdentificationNumber = identificationNumber,
            FullName = fullName,
            BirthDate = birthDate,
            Gender = gender
        };

        return Result.Success(peopleSheet);
    }

    public void UpdateDetails(
        string identificationType,
        string normalizedIdentificationType,
        long affiliateId,
        string identificationNumber,
        string fullName,
        DateTime? birthDate,
        string gender)
    {
        IdentificationType = identificationType;
        NormalizedIdentificationType = normalizedIdentificationType;
        AffiliateId = affiliateId;
        IdentificationNumber = identificationNumber;
        FullName = fullName;
        BirthDate = birthDate;
        Gender = gender;
    }
}
