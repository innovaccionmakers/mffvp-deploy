
namespace Common.SharedKernel.Application.EventBus
{
    public interface IPersonIntegrationEvent
    {
        string DocumentType { get; }
        string Identification { get; }
    }
}