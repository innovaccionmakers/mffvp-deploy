using Common.SharedKernel.Domain;

namespace Products.Domain.Alternatives;

public sealed class Alternative : Entity
{
    private Alternative()
    {
    }

    public long AlternativeId { get; private set; }
    public int AlternativeTypeId { get; private set; }
    public string Name { get; private set; }
    public string Status { get; private set; }
    public string Description { get; private set; }

    public static Result<Alternative> Create(
        int alternativeTypeId, string name, string status, string description
    )
    {
        var alternative = new Alternative
        {
            AlternativeId = default,

            AlternativeTypeId = alternativeTypeId,
            Name = name,
            Status = status,
            Description = description
        };

        alternative.Raise(new AlternativeCreatedDomainEvent(alternative.AlternativeId));
        return Result.Success(alternative);
    }

    public void UpdateDetails(
        int newAlternativeTypeId, string newName, string newStatus, string newDescription
    )
    {
        AlternativeTypeId = newAlternativeTypeId;
        Name = newName;
        Status = newStatus;
        Description = newDescription;
    }
}