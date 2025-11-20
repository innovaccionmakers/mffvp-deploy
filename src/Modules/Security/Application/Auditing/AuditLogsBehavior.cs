using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.Auditing;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Security.Application.Abstractions.Data;
using Security.Application.Abstractions.Services.Auditing;
using Security.Domain.Logs;
using System.Text.Json;

namespace Security.Application.Auditing;

public sealed class AuditLogsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IClientInfoService _clientInfoService;
    private readonly ILogRepository _logRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditLogsBehavior<TRequest, TResponse>> _logger;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IPermissionDescriptionService _permissionDescriptionService;
    private readonly IPreviousStateProvider _previousStateProvider;
    private readonly IAuditLogStore _auditLogStore;

    public AuditLogsBehavior(
        IClientInfoService clientInfoService,
        ILogRepository logRepository,
        IUnitOfWork unitOfWork,
        ILogger<AuditLogsBehavior<TRequest, TResponse>> logger,
        IHttpContextAccessor contextAccessor,
        IPermissionDescriptionService permissionDescriptionService,
        IPreviousStateProvider previousStateProvider,
        IAuditLogStore auditLogStore)
    {
        _clientInfoService = clientInfoService;
        _logRepository = logRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _contextAccessor = contextAccessor;
        _permissionDescriptionService = permissionDescriptionService;
        _previousStateProvider = previousStateProvider;
        _auditLogStore = auditLogStore;
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
        var endpoint = _contextAccessor.HttpContext?.GetEndpoint();
        var policy = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>()?.Policy;

        if (string.IsNullOrWhiteSpace(policy))
        {
            var items = _contextAccessor.HttpContext?.Items;
            if (items is not null && items.TryGetValue("Audit:Policy", out var policyObj))
            {
                policy = policyObj?.ToString();
            }
        }
        var endpointName = endpoint?.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;
        var httpMethod = _contextAccessor.HttpContext?.Request.Method ?? "";
        var action = policy ?? request.GetType().Name;
        var date = DateTime.UtcNow;
        string? description = null;

        if (!string.IsNullOrWhiteSpace(endpointName))
        {
            description = _permissionDescriptionService.GetDescriptionByEndpoint(endpointName, httpMethod);
        }

        if (string.IsNullOrWhiteSpace(description) && !string.IsNullOrWhiteSpace(policy))
        {
            description = _permissionDescriptionService.GetDescriptionByPolicy(policy);
        }

        description ??= "Undefined action";

        var objectData = JsonSerializer.SerializeToDocument(request);

        bool successful = true;
        TResponse? response = default;

        try
        {
            response = await next();
            var evaluatedSuccess = EvaluateSuccessfulProcess(response);
            if (evaluatedSuccess.HasValue)
            {
                successful = evaluatedSuccess.Value;
            }
            return response!;
        }
        catch
        {
            successful = false;
            throw;
        }
        finally
        {
            var previousState = _previousStateProvider.GetSerializedStateAndClear();

            var logResult = Log.Create(
                date,
                action,
                user,
                ip,
                machine,
                description,
                objectData,
                previousState,
                successful);

            if (logResult.IsSuccess)
            {
                _logRepository.Insert(logResult.Value);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var id = logResult.Value.Id;

                if (action == "fvp:accounting:accountingGeneration:generate")
                    await SaveLogReferenceToRedis(id, request, response, cancellationToken);

            }
            else
            {
                _logger.LogWarning("Failed to create log: {Error}", logResult.Error);
            }
        }
    }

    private static bool? EvaluateSuccessfulProcess(object? response)
    {
        return response switch
        {
            Result result => result.IsSuccess,
            GraphqlResult graphqlResult => graphqlResult.Success,
            _ => (bool?)null
        };
    }

    private async Task SaveLogReferenceToRedis(long id, TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        try
        {
            var correlationId = ExtractCorrelationId(request, response);

            if (!string.IsNullOrEmpty(correlationId))
                await _auditLogStore.SaveLogReferenceAsync(id, correlationId, cancellationToken);            
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al guardar referencia de log en Redis");
        }
    }

    private static string? ExtractCorrelationId(TRequest request, TResponse response)
    {
        if (response != null)
        {
            var responseType = response.GetType();

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultValue = responseType.GetProperty("Value")?.GetValue(response);

                if (resultValue != null)
                {
                    var processIdProperty = resultValue.GetType().GetProperty("ProcessId");

                    if (processIdProperty != null)
                        return processIdProperty.GetValue(resultValue)?.ToString();
                }
            }

            var processIdPropertyDirect = responseType.GetProperty("ProcessId");

            if (processIdPropertyDirect != null)
                return processIdPropertyDirect.GetValue(response)?.ToString();
        }

        return null;
    }
}
