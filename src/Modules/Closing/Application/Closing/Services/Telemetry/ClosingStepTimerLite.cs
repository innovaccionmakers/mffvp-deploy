

using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Closing.Application.Closing.Services.Telemetry;

public interface IClosingStepTimer
{
    IDisposable Track(string stepName, int portfolioId, DateTime closingDateUtc, string? executionId = null);
    void AddTag(string key, object? value);
    void SetResultCount(long count);
}

public sealed class ClosingStepTimerLite : IClosingStepTimer
{
    private readonly ILogger<ClosingStepTimerLite> logger;
    private readonly AsyncLocal<StepScope?> current = new();

    public ClosingStepTimerLite(ILogger<ClosingStepTimerLite> logger) => this.logger = logger;

    public IDisposable Track(string stepName, int portfolioId, DateTime closingDateUtc, string? executionId = null)
    {
        var scope = new StepScope(logger, stepName, portfolioId, DateTime.SpecifyKind(closingDateUtc.Date, DateTimeKind.Utc), executionId);
        current.Value = scope;
        return scope;
    }

    public void AddTag(string key, object? value) => current.Value?.AddTag(key, value);
    public void SetResultCount(long count) => current.Value?.AddTag("result.count", count);

    private sealed class StepScope : IDisposable
    {
        private readonly ILogger logger;
        private readonly string stepName;
        private readonly int portfolioId;
        private readonly DateTime closingDateUtc;
        private readonly string? executionId;
        private readonly Stopwatch sw = Stopwatch.StartNew();
        private readonly List<KeyValuePair<string, object?>> tags = new();

        public StepScope(ILogger logger, string stepName, int portfolioId, DateTime closingDateUtc, string? executionId)
        {
            this.logger = logger;
            this.stepName = stepName;
            this.portfolioId = portfolioId;
            this.closingDateUtc = closingDateUtc;
            this.executionId = executionId;

            using var _ = logger.BeginScope(new Dictionary<string, object?>
            {
                ["step"] = stepName,
                ["portfolio.id"] = portfolioId,
                ["date"] = closingDateUtc.ToString("O"),
                ["execution.id"] = executionId ?? "-"
            });

            logger.LogInformation("StepStart | {Step} | Portfolio {PortfolioId} | Date {Date:O} | Execution {ExecutionId}",
                stepName, portfolioId, closingDateUtc, executionId ?? "-");
        }

        public void AddTag(string key, object? value) => tags.Add(new(key, value ?? "null"));

        public void Dispose()
        {
            sw.Stop();
            var extra = tags.Count == 0 ? "-" : string.Join(", ", tags.Select(t => $"{t.Key}={t.Value}"));

            using var _ = logger.BeginScope(new Dictionary<string, object?>
            {
                ["step"] = stepName,
                ["portfolio.id"] = portfolioId,
                ["date"] = closingDateUtc.ToString("O"),
                ["execution.id"] = executionId ?? "-",
                ["elapsed.ms"] = sw.ElapsedMilliseconds
            });

            logger.LogInformation(
                "StepEnd   | {Step} | Duration {ElapsedMs} ms ({Elapsed}) | Portfolio {PortfolioId} | Date {Date:O} | Execution {ExecutionId} | {Extra}",
                stepName, sw.ElapsedMilliseconds, sw.Elapsed, portfolioId, closingDateUtc, executionId ?? "-", extra);
        }
    }
}