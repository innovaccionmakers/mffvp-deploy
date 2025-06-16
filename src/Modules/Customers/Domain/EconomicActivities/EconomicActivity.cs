using Common.SharedKernel.Domain;

namespace Customers.Domain.EconomicActivities;
public sealed class EconomicActivity : Entity
{
    public int EconomicActivityId { get; private set; }
    public string GroupCode { get; private set; }
    public string Description { get; private set; }
    public string CiiuCode { get; private set; }
    public string DivisionCode { get; private set; }
    public string DivisionName { get; private set; }
    public string GroupName { get; private set; }
    public string ClassCode { get; private set; }
    public string HomologatedCode { get; private set; }

    private EconomicActivity() { }

    public static Result<EconomicActivity> Create(
        string groupcode, string description, string ciiucode, string divisioncode, string divisionname, string groupname, string classcode, string homologatedcode
    )
    {
        var economicactivity = new EconomicActivity
        {
                EconomicActivityId = new int(),
                GroupCode = groupcode,
                Description = description,
                CiiuCode = ciiucode,
                DivisionCode = divisioncode,
                DivisionName = divisionname,
                GroupName = groupname,
                ClassCode = classcode,
                HomologatedCode = homologatedcode,
        };
        economicactivity.Raise(new EconomicActivityCreatedDomainEvent(economicactivity.EconomicActivityId));
        return Result.Success(economicactivity);
    }

    public void UpdateDetails(
        string newGroupCode, string newDescription, string newCiiuCode, string newDivisionCode, string newDivisionName, string newGroupName, string newClassCode, string newHomologatedCode
    )
    {
        GroupCode = newGroupCode;
        Description = newDescription;
        CiiuCode = newCiiuCode;
        DivisionCode = newDivisionCode;
        DivisionName = newDivisionName;
        GroupName = newGroupName;
        ClassCode = newClassCode;
        HomologatedCode = newHomologatedCode;
    }
}