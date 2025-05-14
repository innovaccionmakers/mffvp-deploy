namespace Associate.Integrations.Activates;

public sealed record ActivateResponse(
    int ActivateId,
    string IdentificationType,
    string Identification,
    bool Pensioner,
    bool MeetsPensionRequirements,
    DateTime ActivateDate
);