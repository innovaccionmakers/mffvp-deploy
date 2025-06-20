namespace Associate.Integrations.Associates;

public sealed record AssociateResponse(
    string IdentificationType,
    string Identification,
    string FullName
);
