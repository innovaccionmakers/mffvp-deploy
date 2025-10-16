namespace Common.SharedKernel.Domain.NotificationCenter;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class NotificationMessageBody
{
    [JsonPropertyName("IdProceso")]
    public string IdProceso { get; set; }

    [JsonPropertyName("IdPaso")]
    public string IdPaso { get; set; }

    [JsonPropertyName("Administrador")]
    public string Administrador { get; set; }

    [JsonPropertyName("NombreProceso")]
    public string NombreProceso { get; set; }

    [JsonPropertyName("TipoProceso")]
    public string TipoProceso { get; set; } = "Informe";

    [JsonPropertyName("Estado")]
    public string Estado { get; set; }

    [JsonPropertyName("Paso")]
    public string Paso { get; set; }

    [JsonPropertyName("Detalle")]
    public Dictionary<string, string> Detalle { get; set; }

    [JsonPropertyName("EnviaCorreo")]
    public bool EnviaCorreo { get; set; } = false;

    [JsonPropertyName("correoOrigen")]
    public string CorreoOrigen { get; set; }

    [JsonPropertyName("Destinatario")]
    public List<Recipient> Destinatario { get; set; }
}
