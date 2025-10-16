namespace Common.SharedKernel.Domain.NotificationCenter;
using System.Text.Json.Serialization;

public class Recipient
{
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("usuario")]
    public string Usuario { get; set; }
}
