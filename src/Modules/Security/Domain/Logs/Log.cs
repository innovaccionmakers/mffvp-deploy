using System.Text.Json;
using Common.SharedKernel.Domain;

namespace Security.Domain.Logs;

public sealed class Log : Entity
{
    public long Id { get; private set; }
    public DateTime Date { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string User { get; private set; } = string.Empty;
    public string Ip { get; private set; } = string.Empty;
    public string Machine { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public JsonDocument ObjectData { get; private set; } = null!;
    public JsonDocument PreviousObjectState { get; private set; } = null!;
    public bool SuccessfulProcess { get; private set; }

    private Log() { }

    public static Result<Log> Create(
        DateTime date,
        string action,
        string user,
        string ip,
        string machine,
        string description,
        JsonDocument objectData,
        JsonDocument previousObjectState,
        bool successfulProcess)
    {
        var log = new Log
        {
            Date = date,
            Action = action,
            User = user,
            Ip = ip,
            Machine = machine,
            Description = description,
            ObjectData = objectData,
            PreviousObjectState = previousObjectState,
            SuccessfulProcess = successfulProcess
        };

        return Result.Success(log);
    }

    public static Result<Log> UpdateSuccessStatus(long id, bool success)
    {
        var log = new Log
        {
            Id = id,
            SuccessfulProcess = success
        };

        return Result.Success(log);
    }
}
