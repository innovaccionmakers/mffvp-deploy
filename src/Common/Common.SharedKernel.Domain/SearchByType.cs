using System.Text.Json.Serialization;

namespace Common.SharedKernel.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SearchByType
{
    Nombre,
    Identificacion
}
