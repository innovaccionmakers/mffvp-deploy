using Operations.Integrations.Contributions;

namespace Operations.Presentation.DTOs;

public record ContributionMutationResult(
    bool Success,
    string? Message = null,
    ContributionResponse? Contribution = null);