namespace Accounting.Integrations.ConsecutivesSetup;

public sealed record ConsecutiveSetupResponse(
    long Id,
    string Nature,
    string SourceDocument,
    int Consecutive);
