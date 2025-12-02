namespace Accounting.Presentation.DTOs;

public sealed record ConsecutiveSetupPayloadDto(
    IReadOnlyCollection<ConsecutiveSetupDto>? Consecutives,
    ConsecutiveSetupDto? UpdatedConsecutive);
