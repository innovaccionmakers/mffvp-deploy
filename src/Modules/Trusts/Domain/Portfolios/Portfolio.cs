namespace Trusts.Domain.Portfolios;

public sealed record Portfolio(string Code, string Name, bool IsCollector, int ObjectiveId, DateTime OperationDate);