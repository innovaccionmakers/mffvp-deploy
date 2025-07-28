using System.Text.Json;
using System.Linq;
using Common.SharedKernel.Application.Attributes;
using MediatR;
using Microsoft.Extensions.Logging;
using Security.Application.Abstractions.Data;
using Security.Application.Abstractions.Services.Auditing;
using Security.Domain.Logs;

namespace Security.Application.Auditing;

public sealed class AuditLogsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IClientInfoService _clientInfoService;
    private readonly ILogRepository _logRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditLogsBehavior<TRequest, TResponse>> _logger;

    public AuditLogsBehavior(
        IClientInfoService clientInfoService,
        ILogRepository logRepository,
        IUnitOfWork unitOfWork,
        ILogger<AuditLogsBehavior<TRequest, TResponse>> logger)
    {
        _clientInfoService = clientInfoService;
        _logRepository = logRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var attribute = (AuditLogAttribute?)request.GetType().GetCustomAttributes(typeof(AuditLogAttribute), true).FirstOrDefault();
        if (attribute is null)
        {
            return await next();
        }

        var user = _clientInfoService.GetUserName();
        var ip = _clientInfoService.GetClientIpAddress();
        var machine = Environment.MachineName;
        var action = request.GetType().Name;
        var date = DateTime.UtcNow;
        var description = _clientInfoService.GetActionDescription();

        if (string.IsNullOrWhiteSpace(description))
        {
            description = attribute.Description;
        }

        var objectData = JsonSerializer.SerializeToDocument(request);

        bool successful = true;
        TResponse? response = default;

        try
        {
            response = await next();
            return response!;
        }
        catch
        {
            successful = false;
            throw;
        }
        finally
        {
            var logResult = Log.Create(
                date,
                action,
                user,
                ip,
                machine,
                description,
                objectData,
                JsonDocument.Parse("{}"),
                successful);

            if (logResult.IsSuccess)
            {
                _logRepository.Insert(logResult.Value);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                _logger.LogWarning("Failed to create log: {Error}", logResult.Error);
            }
        }
    }
}