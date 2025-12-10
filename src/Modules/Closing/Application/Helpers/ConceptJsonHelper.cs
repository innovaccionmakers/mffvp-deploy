using Closing.Application.PreClosing.Services.Yield.Dto;
using Closing.Domain.ConfigurationParameters;
using Common.SharedKernel.Application.Helpers.Serialization;
using System.Text.Json;

namespace Closing.Application.Helpers;

public static class ConceptJsonHelper
{
    public static async Task<IEnumerable<string>> BuildConceptJsonsAsync(
        IConfigurationParameterRepository configurationParameterRepository,
        IEnumerable<Guid> guidConcepts,
        CancellationToken cancellationToken)
    {
        if (guidConcepts == null || !guidConcepts.Any())
            return Enumerable.Empty<string>();

        var conceptParams = await configurationParameterRepository.GetReadOnlyByUuidsAsync(guidConcepts, cancellationToken);

        var conceptJsons = new List<string>();

        foreach (var conceptParam in conceptParams.Values)
        {
            if (conceptParam?.Metadata == null)
                continue;

            var conceptId = JsonIntegerHelper.ExtractInt32(conceptParam.Metadata, "id", defaultValue: 0);
            var conceptName = JsonStringHelper.ExtractString(conceptParam.Metadata, "nombre", defaultValue: string.Empty);

            if (conceptId <= 0 || string.IsNullOrWhiteSpace(conceptName))
                continue;

            var conceptDto = new StringEntityDto(conceptId.ToString(), conceptName);
            conceptJsons.Add(JsonSerializer.Serialize(conceptDto));
        }

        return conceptJsons;
    }
}

