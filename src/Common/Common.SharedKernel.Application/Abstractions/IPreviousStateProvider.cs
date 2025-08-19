namespace Common.SharedKernel.Application.Abstractions;

public interface IPreviousStateProvider
{
    void AddState(string entityName, IDictionary<string, object?> values);

    System.Text.Json.JsonDocument GetSerializedStateAndClear();
}