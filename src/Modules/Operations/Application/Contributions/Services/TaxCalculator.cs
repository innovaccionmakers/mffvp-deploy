using System.Globalization;
using System.Text.Json;
using Operations.Application.Abstractions.Data;
using Operations.Application.Contributions.CreateContribution;
using Operations.Domain.ConfigurationParameters;

namespace Operations.Application.Contributions.Services;

public sealed class TaxCalculator(IConfigurationParameterRepository cfgRepo) : ITaxCalculator
{
    public async Task<TaxResult> ComputeAsync(bool pensioner, bool certified, decimal amount, CancellationToken ct)
    {
        var taxUuid = ConfigurationParameterUuids.TaxRetention;
        if (pensioner)
            taxUuid = ConfigurationParameterUuids.TaxExempt;
        else if (certified) taxUuid = ConfigurationParameterUuids.TaxNoRetention;

        var stateUuid = certified
            ? ConfigurationParameterUuids.CertifiedState
            : ConfigurationParameterUuids.UncertifiedState;

        var uuids = new List<Guid> { taxUuid, stateUuid };

        if (!pensioner && !certified)
            uuids.Add(ConfigurationParameterUuids.RetentionPct);

        var cfgs = await cfgRepo.GetByUuidsAsync(uuids, ct);

        var pct = 0m;
        if (cfgs.TryGetValue(ConfigurationParameterUuids.RetentionPct, out var pctParam))
            pct = ExtractPercent(pctParam.Metadata);

        var withheld = pct > 0 ? amount * pct : 0m;

        return new TaxResult(
            cfgs[taxUuid].ConfigurationParameterId,
            cfgs[stateUuid].ConfigurationParameterId,
            withheld,
            cfgs[taxUuid].Name);
    }

    internal static decimal ExtractPercent(JsonDocument metadata)
    {
        if (metadata is null) return 0m;

        var root = metadata.RootElement;

        if (root.ValueKind == JsonValueKind.String)
            return ParseFraction(root.GetString());

        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("valor", out var valueElement) &&
            valueElement.ValueKind == JsonValueKind.String)
            return ParseFraction(valueElement.GetString());

        if (root.ValueKind == JsonValueKind.Number &&
            root.TryGetDecimal(out var number))
            return number / 100m;

        return 0m;
    }

    internal static decimal ParseFraction(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return 0m;

        var sanitized = raw.Replace("%", "", StringComparison.Ordinal).Trim();

        sanitized = sanitized.Replace(',', '.');

        return decimal.TryParse(sanitized, NumberStyles.Any, CultureInfo.InvariantCulture, out var val)
            ? val / 100m
            : 0m;
    }
}