namespace Common.SharedKernel.Application.Closing;

public sealed record ClosingExecutionState(
    DateTime ClosingDatetime,
    DateTime ProcessDatetime,
    ClosingProcess Process);
