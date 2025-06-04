using Common.SharedKernel.Domain;
using Products.Domain.AlternativePortfolios;
using Products.Domain.Objectives;

namespace Products.Domain.Alternatives;

public sealed class Alternative : Entity
{
    public int AlternativeId { get; private set; }
    public int AlternativeTypeId { get; private set; }
    public string Name { get; private set; }
    public string Status { get; private set; }
    public string Description { get; private set; }
    public string HomologatedCode { get; private set; }


    private readonly List<AlternativePortfolio> _portfolios = [];
    public IReadOnlyCollection<AlternativePortfolio> Portfolios => _portfolios;


    private readonly List<Objective> _objectives = [];
    public IReadOnlyCollection<Objective> Objectives => _objectives;

    private Alternative()
    {
    }

    public static Result<Alternative> Create(
        int alternativeTypeId,
        string name,
        string status,
        string description,
        string homologatedCode
    )
    {
        var alternative = new Alternative
        {
            AlternativeId = default,

            AlternativeTypeId = alternativeTypeId,
            Name = name,
            Status = status,
            Description = description,
            HomologatedCode = homologatedCode
        };

        alternative.Raise(new AlternativeCreatedDomainEvent(alternative.AlternativeId));
        return Result.Success(alternative);
    }

    public void UpdateDetails(
        int newAlternativeTypeId,
        string newName,
        string newStatus,
        string newDescription,
        string newHomologatedCode
    )
    {
        AlternativeTypeId = newAlternativeTypeId;
        Name = newName;
        Status = newStatus;
        Description = newDescription;
        HomologatedCode = newHomologatedCode;
    }
}