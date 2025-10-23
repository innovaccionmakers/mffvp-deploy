using Amazon.SQS;
using Amazon.SQS.Model;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Domain.Aws;
using Common.SharedKernel.Domain.Constants;
using Common.SharedKernel.Domain.NotificationsCenter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace Common.SharedKernel.Infrastructure.NotificationsCenter;

public class NotificationCenter(
    IAmazonSQS sqsClient,
    IOptions<SqsConfig> sqsConfig,
    ILogger<NotificationCenter> logger,
    IConfiguration configuration) : INotificationCenter
{
    private readonly string origin = configuration["NotificationSettings:Origin"] ?? NotificationDefaults.Origin;
    public async Task SendNotificationAsync(string user, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            logger.LogWarning("Intento de enviar mensaje vacío o nulo");
            return;
        }

        try
        {
            var request = new SendMessageRequest
            {
                QueueUrl = sqsConfig.Value.QueueUrl,
                MessageBody = message,
                MessageAttributes = BuildMessageAttributes(origin, user),
                MessageGroupId = NotificationConstants.MessageGroupId,
                MessageDeduplicationId = Guid.NewGuid().ToString()
            };

            var response = await sqsClient.SendMessageAsync(request, cancellationToken);

            logger.LogInformation("Mensaje enviado exitosamente a SQS. MessageId: {MessageId}", response.MessageId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al enviar mensaje a SQS: {Message}", message);
            throw;
        }
    }


    public async Task SendNotificationAsync(string user, string message, Dictionary<string, string> metadata, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            logger.LogWarning("Intento de enviar mensaje vacío o nulo");
            return;
        }

        try
        {
            var messageAttributes = BuildMessageAttributes(origin, user);


            foreach (var (key, value) in metadata)
            {
                messageAttributes[key] = new MessageAttributeValue
                {
                    StringValue = value,
                    DataType = "String"
                };
            }

            var request = new SendMessageRequest
            {
                QueueUrl = sqsConfig.Value.QueueUrl,
                MessageBody = message,
                MessageAttributes = messageAttributes,
                MessageGroupId = NotificationConstants.MessageGroupId,
                MessageDeduplicationId = Guid.NewGuid().ToString()
            };

            var response = await sqsClient.SendMessageAsync(request, cancellationToken);

            logger.LogInformation("Mensaje con metadatos enviado exitosamente a SQS. MessageId: {MessageId}", response.MessageId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al enviar mensaje con metadatos a SQS: {Message}", message);
            throw;
        }
    }


    public async Task SendNotificationAsync<T>(string user, T payload, CancellationToken cancellationToken = default) where T : class
    {
        if (payload == null)
        {
            logger.LogWarning("Intento de enviar payload nulo");
            return;
        }

        try
        {
            var jsonMessage = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            var request = new SendMessageRequest
            {
                QueueUrl = sqsConfig.Value.QueueUrl,
                MessageBody = jsonMessage,
                MessageAttributes = BuildMessageAttributes(origin, user),
                MessageGroupId = NotificationConstants.MessageGroupId,
                MessageDeduplicationId = Guid.NewGuid().ToString()
            };

            var response = await sqsClient.SendMessageAsync(request, cancellationToken);

            logger.LogInformation("Objeto {PayloadType} enviado exitosamente a SQS. MessageId: {MessageId}",
                typeof(T).Name, response.MessageId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al enviar objeto {PayloadType} a SQS", typeof(T).Name);
            throw;
        }
    }

    private static Dictionary<string, MessageAttributeValue> BuildMessageAttributes(string origin, string user)
    {
        return new Dictionary<string, MessageAttributeValue>
        {
            { "versionEvento", new MessageAttributeValue { DataType = "String", StringValue = NotificationConstants.CurrentVersion }},
            { "origen", new MessageAttributeValue { DataType = "String", StringValue = origin }},
            { "fechaEvento", new MessageAttributeValue { DataType = "String", StringValue = DateTime.UtcNow.AddSeconds(1).ToString("o") }},
            { "tipoEvento", new MessageAttributeValue { DataType = "String", StringValue = NotificationConstants.EventType }},
            { "usuario", new MessageAttributeValue { DataType = "String", StringValue = user }}
        };
    }

    public static NotificationMessageBody BuildMessageBody(
        string processId,
        string stepId,
        string administrator,
        string processName,
        string processType,
        string status,
        string stepDescription,
        object details,
        bool? sendEmail = null,
        string? emailFrom = null,
        List<Recipient>? recipients = null)
    {
        return new NotificationMessageBody
        {
            IdProceso = processId,
            IdPaso = stepId,
            Administrador = administrator,
            NombreProceso = processName,
            TipoProceso = processType,
            Estado = status,
            Paso = stepDescription,
            Detalle = details,
            EnviaCorreo = sendEmail,
            CorreoOrigen = emailFrom,
            Destinatario = recipients
        };
    }
}