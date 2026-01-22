namespace Reports.Infrastructure.Configuration;

/// <summary>
/// Opciones de configuración para timeouts de base de datos.
/// </summary>
public sealed class DatabaseTimeoutsOptions
{
    /// <summary>
    /// Nombre de la sección en appsettings.json
    /// </summary>
    public const string SectionName = "CustomSettings:DatabaseTimeouts";
    
    /// <summary>
    /// Timeout en segundos para comandos de base de datos.
    /// Valor por defecto: 120 segundos.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 120;
}