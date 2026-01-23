using Reports.Domain.LoadingInfo.Audit;
using Reports.Domain.LoadingInfo.Audit.Dto;
using System.Text.Json;

namespace Reports.Application.LoadingInfo.Audit;

public sealed class ExecutionParametersBuilder
{
    private static class InputKeys
    {
        public const string PortfolioId = "portfolioId";
        public const string ClosingDateUtc = "closingDateUtc";
        public const string FundId = "fundId";
        public const string GoalId = "goalId";
        public const string AffiliateId = "affiliateId";
    }

    private static class OptionKeys
    {
        public const string ChunkSize = "chunkSize";
        public const string DryRun = "dryRun";
        public const string Force = "force";
        public const string MaxDegreeOfParallelism = "maxDegreeOfParallelism";
    }

    private readonly object syncLock = new();

    private readonly string etlSelection;
    private readonly List<string> etlNames;

    private readonly Dictionary<string, string> inputs = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> options = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, EtlRunSummaryDto> etlSummaries = new(StringComparer.OrdinalIgnoreCase);

    private string? requestedBy;
    private string? correlationId;
    private DateTimeOffset requestedAtUtc;

    public ExecutionParametersBuilder(string etlSelection, IEnumerable<string> etlNames)
    {
        this.etlSelection = NormalizeSelection(etlSelection);
        this.etlNames = etlNames
            .Select(NormalizeName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(EtlPayloadLimits.MaxEtls)
            .ToList();

        requestedAtUtc = DateTimeOffset.UtcNow;

        foreach (var etlName in this.etlNames)
        {
            etlSummaries[etlName] = new EtlRunSummaryDto(
                EtlName: etlName,
                Status: EtlRunStatus.Pending,
                DurationMilliseconds: null,
                Metrics: null,
                WarningCodes: null
            );
        }
    }

    // ------- metadata -------

    public ExecutionParametersBuilder WithRequestedBy(string? value)
    {
        requestedBy = SafeValue(value);
        return this;
    }

    public ExecutionParametersBuilder WithCorrelationId(string? value)
    {
        correlationId = SafeValue(value);
        return this;
    }

    public ExecutionParametersBuilder WithRequestedAtUtc(DateTimeOffset value)
    {
        requestedAtUtc = value;
        return this;
    }

    public ExecutionParametersBuilder WithPortfolioId(int portfolioId)
    {
        if (portfolioId > 0)
            SetInput(InputKeys.PortfolioId, portfolioId.ToString());
        return this;
    }

    public ExecutionParametersBuilder WithClosingDateUtc(DateTime closingDateUtc)
    {
        SetInput(InputKeys.ClosingDateUtc, closingDateUtc.ToString("yyyy-MM-dd"));
        return this;
    }

    public ExecutionParametersBuilder WithFundId(int fundId)
    {
        if (fundId > 0)
            SetInput(InputKeys.FundId, fundId.ToString());
        return this;
    }

    public ExecutionParametersBuilder WithGoalId(long goalId)
    {
        if (goalId > 0)
            SetInput(InputKeys.GoalId, goalId.ToString());
        return this;
    }

    public ExecutionParametersBuilder WithAffiliateId(long affiliateId)
    {
        if (affiliateId > 0)
            SetInput(InputKeys.AffiliateId, affiliateId.ToString());
        return this;
    }


    public ExecutionParametersBuilder WithChunkSize(int chunkSize)
    {
        if (chunkSize > 0)
            SetOption(OptionKeys.ChunkSize, chunkSize.ToString());
        return this;
    }

    public ExecutionParametersBuilder WithDryRun(bool dryRun)
    {
        SetOption(OptionKeys.DryRun, dryRun ? "true" : "false");
        return this;
    }

    public ExecutionParametersBuilder WithForce(bool force)
    {
        SetOption(OptionKeys.Force, force ? "true" : "false");
        return this;
    }

    public ExecutionParametersBuilder WithMaxDegreeOfParallelism(int maxDegreeOfParallelism)
    {
        if (maxDegreeOfParallelism > 0)
            SetOption(OptionKeys.MaxDegreeOfParallelism, maxDegreeOfParallelism.ToString());
        return this;
    }

    public void MarkRunning(string etlName)
        => UpdateSummary(etlName, current => current with { Status = EtlRunStatus.Running });

    public void MarkCompleted(
        string etlName,
        long durationMilliseconds,
        IDictionary<string, long>? metrics = null,
        IEnumerable<string>? warningCodes = null)
        => UpdateSummary(etlName, current => current with
        {
            Status = EtlRunStatus.Completed,
            DurationMilliseconds = durationMilliseconds,
            Metrics = TrimMetrics(metrics),
            WarningCodes = TrimWarnings(warningCodes)
        });

    public void MarkFailed(string etlName, long durationMilliseconds, IEnumerable<string>? warningCodes = null)
        => UpdateSummary(etlName, current => current with
        {
            Status = EtlRunStatus.Failed,
            DurationMilliseconds = durationMilliseconds,
            Metrics = null,
            WarningCodes = TrimWarnings(warningCodes)
        });

    public void MarkSkipped(string etlName)
        => UpdateSummary(etlName, current => current with { Status = EtlRunStatus.Skipped });

    public ExecutionParametersDto Build()
    {
        lock (syncLock)
        {
            var runs = etlNames
                .Select(name => etlSummaries.TryGetValue(name, out var summary)
                    ? summary
                    : new EtlRunSummaryDto(name, EtlRunStatus.Pending, null, null, null))
                .ToList();

            return new ExecutionParametersDto(
                EtlSelection: etlSelection,
                EtlNames: etlNames,
                RequestedBy: requestedBy,
                RequestedAtUtc: requestedAtUtc,
                CorrelationId: correlationId,
                Inputs: new Dictionary<string, string>(inputs, StringComparer.OrdinalIgnoreCase),
                Options: options.Count == 0 ? null : new Dictionary<string, string>(options, StringComparer.OrdinalIgnoreCase),
                EtlRuns: runs.Count == 0 ? null : runs
            );
        }
    }

    public JsonDocument BuildFinalJsonDocument()
        => EtlJson.ToJsonDocument(Build());

    private void UpdateSummary(string etlName, Func<EtlRunSummaryDto, EtlRunSummaryDto> update)
    {
        var normalized = NormalizeName(etlName);
        if (string.IsNullOrWhiteSpace(normalized))
            return;

        lock (syncLock)
        {
            if (!etlSummaries.TryGetValue(normalized, out var current))
                return;

            etlSummaries[normalized] = update(current);
        }
    }

    private void SetInput(string key, string value)
    {
        var safeKey = SafeKey(key);
        var safeValue = SafeValue(value);

        if (string.IsNullOrWhiteSpace(safeKey) || string.IsNullOrWhiteSpace(safeValue))
            return;

        lock (syncLock)
        {
            if (inputs.Count >= EtlPayloadLimits.MaxKeyValues && !inputs.ContainsKey(safeKey))
                return;

            inputs[safeKey] = safeValue;
        }
    }

    private void SetOption(string key, string value)
    {
        var safeKey = SafeKey(key);
        var safeValue = SafeValue(value);

        if (string.IsNullOrWhiteSpace(safeKey) || string.IsNullOrWhiteSpace(safeValue))
            return;

        lock (syncLock)
        {
            if (options.Count >= EtlPayloadLimits.MaxKeyValues && !options.ContainsKey(safeKey))
                return;

            options[safeKey] = safeValue;
        }
    }

    private static IReadOnlyDictionary<string, long>? TrimMetrics(IDictionary<string, long>? metrics)
    {
        if (metrics is null || metrics.Count == 0)
            return null;

        return metrics
            .Take(EtlPayloadLimits.MaxMetricsPerEtl)
            .ToDictionary(x => SafeKey(x.Key), x => x.Value, StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<string>? TrimWarnings(IEnumerable<string>? warnings)
    {
        if (warnings is null)
            return null;

        var list = warnings
            .Select(SafeValue)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(EtlPayloadLimits.MaxWarningsPerEtl)
            .ToList()!;

        return list.Count == 0 ? null : list;
    }

    private static string NormalizeName(string name)
        => SafeValue(name)?.Trim().ToLowerInvariant() ?? string.Empty;

    private static string NormalizeSelection(string selection)
    {
        var normalized = NormalizeName(selection);
        return normalized == "all" ? "all" : "custom";
    }

    private static string SafeKey(string? value)
        => (SafeValue(value) ?? string.Empty).Trim().ToLowerInvariant();

    private static string? SafeValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= EtlPayloadLimits.MaxValueLength
            ? trimmed
            : trimmed[..EtlPayloadLimits.MaxValueLength];
    }
}