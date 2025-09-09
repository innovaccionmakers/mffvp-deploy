using DotNetCore.CAP.Filter;

using Microsoft.Extensions.Logging;

namespace Common.SharedKernel.Infrastructure.EventBus;

public class CapsEventsFilter : ISubscribeFilter
{
    private readonly ILogger<CapsEventsFilter> _logger;

    public CapsEventsFilter(ILogger<CapsEventsFilter> logger)
    {
        _logger = logger;
    }

    public Task OnSubscribeExecutingAsync(ExecutingContext context)
    {
        _logger.Log(LogLevel.Information, "EVENTO CONSUMIDO {0}", context.ConsumerDescriptor.TopicName);

        return Task.CompletedTask;
    }

    public Task OnSubscribeExecutedAsync(ExecutedContext context)
    {
        _logger.Log(LogLevel.Information, "EVENTO EJECUTADO {0}", context.ConsumerDescriptor.TopicName);
        return Task.CompletedTask;
    }

    public Task OnSubscribeExceptionAsync(ExceptionContext context)
    {
        //_logger.LogError(LogLevel.Information, "EVENTO {0}, ERROR {1}", context.ConsumerDescriptor.TopicName,context.Exception.Message);
        return Task.CompletedTask;
    }
}
