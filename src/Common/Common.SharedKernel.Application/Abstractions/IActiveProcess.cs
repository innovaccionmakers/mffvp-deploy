namespace Common.SharedKernel.Application.Abstractions
{
    public interface IActiveProcess
    {
        Task<bool> GetProcessActiveAsync(CancellationToken cancellationToken = default);
        Task SaveProcessActiveAsync(CancellationToken cancellationToken = default);
        Task RemoveProcessActiveAsync(CancellationToken cancellationToken = default);
    }
}
