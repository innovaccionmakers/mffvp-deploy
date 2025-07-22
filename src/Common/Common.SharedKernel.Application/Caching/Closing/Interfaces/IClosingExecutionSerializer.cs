namespace Common.SharedKernel.Application.Caching.Closing.Interfaces;

public interface IClosingExecutionSerializer
{
    string Serialize(ClosingExecutionState state);
    ClosingExecutionState? Deserialize(string json);
}
