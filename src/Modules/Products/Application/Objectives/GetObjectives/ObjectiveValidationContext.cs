namespace Products.Application.Objectives.GetObjectives;

public sealed class ObjectiveValidationContext
{
    public bool AffiliateExists { get; init; }
    public bool DocumentTypeExists { get; set; }
    public bool RequestedStatusAccepted { get; init; }
    public bool AffiliateHasObjectives { get; init; }
    public bool AffiliateHasActive { get; init; }
    public bool AffiliateHasInactive { get; init; }
    public string RequestedStatus { get; init; } = "";
}