using Operations.Domain.ConfigurationParameters;
using Operations.Application.Abstractions.Data;
using Operations.Application.Contributions.CreateContribution;
using Common.SharedKernel.Domain.ConfigurationParameters;

namespace Operations.Application.Contributions.Services;

public sealed class TaxCalculator(IConfigurationParameterRepository cfgRepo) : ITaxCalculator
{
    public async Task<TaxResult> ComputeAsync(bool pensioner, bool certified, decimal amount, CancellationToken ct)
    {
        var uuids = new List<Guid>
        {
            pensioner
                ? ConfigurationParameterUuids.TaxExempt
                : certified
                    ? ConfigurationParameterUuids.TaxNoRetention
                    : ConfigurationParameterUuids.TaxRetention,
            certified
                ? ConfigurationParameterUuids.CertifiedState
                : ConfigurationParameterUuids.UncertifiedState
        };
        if (!pensioner && !certified)
            uuids.Add(ConfigurationParameterUuids.RetentionPct);

        var cfgs = await cfgRepo.GetByUuidsAsync(uuids, ct);

        var pct = 0m;
        if (cfgs.TryGetValue(ConfigurationParameterUuids.RetentionPct, out var pctParam))
            if (decimal.TryParse(pctParam.Metadata.RootElement.GetString()?.TrimEnd('%'), out var p))
                pct = p / 100m;

        var withheld = pct > 0 ? amount * pct : 0m;

        return new TaxResult(
            cfgs[uuids[0]].ConfigurationParameterId,
            cfgs[uuids[1]].ConfigurationParameterId,
            withheld,
            cfgs[uuids[0]].Name);
    }
}