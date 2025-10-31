using System.Text.Json;

namespace Closing.Application.PreClosing.Services.ExtraReturns.Dto;

public sealed record TrustOperationRemoteResponse(
    int PortfolioId,
    DateTime ProcessDateUtc,
    long OperationTypeId,
    string OperationTypeName,
    decimal Amount);

public sealed record ExtraReturnSummary(
    int PortfolioId,
    DateTime ProcessDateUtc,
    long OperationTypeId,
    string OperationTypeName,
    decimal Amount,
    string Concept);

public static class ExtraReturnSummaryExtensions
{
    public static JsonDocument ToJsonSummary(this ExtraReturnSummary summary)
    {
        return JsonDocument.Parse(summary.Concept);
    }
}
