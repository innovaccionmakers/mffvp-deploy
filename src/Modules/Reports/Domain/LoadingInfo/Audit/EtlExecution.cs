using Common.SharedKernel.Domain;
using System.Text.Json;

namespace Reports.Domain.LoadingInfo.Audit;

public sealed class EtlExecution : Entity
{
    public long Id { get; private set; } // id (identity)
    public string Name { get; private set; } = string.Empty; // carga_nombre

    public JsonDocument? Parameters { get; private set; } // parametros
    public string Status { get; private set; } = EtlExecutionStatus.Running; // estado

    public DateTimeOffset StartedAtUtc { get; private set; } // fecha_inicio (timestamptz)
    public DateTimeOffset? FinishedAtUtc { get; private set; } // fecha_fin (timestamptz)

    public long? DurationMilliseconds { get; private set; } // duracion
    public JsonDocument? Error { get; private set; } // error

    private EtlExecution()
    { }

    public static EtlExecution Start(string name, JsonDocument? parameters, DateTimeOffset startedAtUtc)
    {
        return new EtlExecution
        {
            Name = name,
            Parameters = parameters,
            Status = EtlExecutionStatus.Running,
            StartedAtUtc = startedAtUtc
        };
    }
}