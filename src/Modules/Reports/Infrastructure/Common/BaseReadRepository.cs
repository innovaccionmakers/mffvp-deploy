using Microsoft.Extensions.Options;
namespace Reports.Infrastructure.Common;

/// <summary>
/// Clase base abstracta para repositorios de lectura que proporciona
/// configuración común como timeouts de comandos de base de datos.
/// </summary>
public abstract class BaseReadRepository
{
    /// <summary>
    /// Timeout en segundos para comandos de base de datos.
    /// </summary>
    protected int CommandTimeoutSeconds { get; }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="BaseReadRepository"/>.
    /// </summary>
    /// <param name="timeoutOptions">Opciones de configuración de timeouts.</param>
    protected BaseReadRepository(IOptions<Reports.Infrastructure.Configuration.DatabaseTimeoutsOptions> timeoutOptions)
    {
        ArgumentNullException.ThrowIfNull(timeoutOptions);
        CommandTimeoutSeconds = timeoutOptions.Value.CommandTimeoutSeconds;
    }
}