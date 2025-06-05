using Common.SharedKernel.Domain;

namespace People.Domain.EconomicActivities;

public sealed class EconomicActivity : Entity
{
    public string EconomicActivityId { get; private set; }
    public string Description { get; private set; }
    public string CiiuCode { get; private set; }
    public string DivisionCode { get; private set; }
    public string DivisionName { get; private set; }
    public string GroupName { get; private set; }
    public string ClassCode { get; private set; }
    public string HomologatedCode { get; private set; }

    private EconomicActivity()
    {
    }

    public static Result<EconomicActivity> Create(
        string economicActivityId,
        string description,
        string ciiuCode,
        string divisionCode,
        string divisionName,
        string groupName,
        string classCode,
        string homologatedCode
    )
    {
        var economicactivity = new EconomicActivity
        {
            EconomicActivityId = economicActivityId,
            Description = description,
            CiiuCode = ciiuCode,
            DivisionCode = divisionCode,
            DivisionName = divisionName,
            GroupName = groupName,
            ClassCode = classCode,
            HomologatedCode = homologatedCode
        };

        economicactivity.Raise(new EconomicActivityCreatedDomainEvent(economicactivity.EconomicActivityId));
        return Result.Success(economicactivity);
    }

    public void UpdateDetails(
        string newEconomicActivityId,
        string newDescription,
        string newCiiuCode,
        string newDivisionCode,
        string newDivisionName,
        string newGroupName,
        string newClassCode,
        string newHomologatedCode
    )
    {
        EconomicActivityId = newEconomicActivityId;
        Description = newDescription;
        CiiuCode = newCiiuCode;
        DivisionCode = newDivisionCode;
        DivisionName = newDivisionName;
        GroupName = newGroupName;
        ClassCode = newClassCode;
        HomologatedCode = newHomologatedCode;
    }
}