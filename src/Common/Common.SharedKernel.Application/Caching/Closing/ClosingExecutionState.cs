namespace Common.SharedKernel.Application.Caching.Closing;

public sealed record ClosingExecutionState(
    DateTime ClosingDatetime,
    DateTime ProcessDatetime,
    string Process);
