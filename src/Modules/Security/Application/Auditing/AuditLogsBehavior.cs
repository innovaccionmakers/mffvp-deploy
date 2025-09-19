using System.Text.Json;
using System.Linq;
using System.Reflection;
using Common.SharedKernel.Application.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Security.Application.Abstractions.Data;
using Security.Application.Abstractions.Services.Auditing;
using Common.SharedKernel.Application.Abstractions;
using Security.Domain.Logs;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Presentation.Results;

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

    public AuditLogsBehavior(
        IClientInfoService clientInfoService,
        ILogRepository logRepository,
        IUnitOfWork unitOfWork,
        ILogger<AuditLogsBehavior<TRequest, TResponse>> logger,
        IHttpContextAccessor contextAccessor,
        IPermissionDescriptionService permissionDescriptionService,
        IPreviousStateProvider previousStateProvider)
    {
        _clientInfoService = clientInfoService;
        _logRepository = logRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _contextAccessor = contextAccessor;
        _permissionDescriptionService = permissionDescriptionService;
        _previousStateProvider = previousStateProvider;
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
}
