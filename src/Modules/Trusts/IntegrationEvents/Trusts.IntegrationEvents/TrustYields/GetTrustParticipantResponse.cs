namespace Trusts.IntegrationEvents.TrustYields;

public sealed record GetTrustParticipantResponse(
    bool IsValid,
    string? Code,
    string? Message, int Participants);
