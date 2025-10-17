using Amazon.S3;
using Amazon.S3.Model;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Domain.Aws;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace Common.SharedKernel.Infrastructure.FileStorage;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Config _s3Config;
    private readonly ILogger<S3FileStorageService> _logger;

    public S3FileStorageService(
        IAmazonS3 s3Client,
        IOptions<S3Config> s3Config,
        ILogger<S3FileStorageService> logger)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        _s3Config = s3Config?.Value ?? throw new ArgumentNullException(nameof(s3Config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> UploadFileAsync(
        byte[] fileContent,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        if (fileContent == null || fileContent.Length == 0)
        {
            throw new ArgumentException("El contenido del archivo no puede estar vacío", nameof(fileContent));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("El nombre del archivo es requerido", nameof(fileName));
        }

        // Validar tamaño del archivo
        var fileSizeMB = fileContent.Length / (1024.0 * 1024.0);
        if (fileSizeMB > _s3Config.MaxFileSizeMB)
        {
            throw new InvalidOperationException($"El archivo excede el tamaño máximo permitido de {_s3Config.MaxFileSizeMB}MB");
        }

        try
        {
            var fileKey = GenerateFileKey(fileName, folder);

            var request = new PutObjectRequest
            {
                BucketName = _s3Config.BucketName,
                Key = fileKey,
                InputStream = new MemoryStream(fileContent),
                ContentType = contentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            // Agregar metadatos
            request.Metadata.Add("uploaded-at", DateTime.UtcNow.ToString("O"));
            request.Metadata.Add("original-filename", fileName);
            request.Metadata.Add("file-size", fileContent.Length.ToString());

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);

            var publicUrl = GetPublicUrl(fileKey);

            _logger.LogInformation("Archivo subido exitosamente a S3. Key: {FileKey}, Size: {FileSize} bytes",
                fileKey, fileContent.Length);

            return publicUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir archivo {FileName} a S3", fileName);
            throw;
        }
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        if (fileStream == null)
        {
            throw new ArgumentNullException(nameof(fileStream));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("El nombre del archivo es requerido", nameof(fileName));
        }

        try
        {
            var fileKey = GenerateFileKey(fileName, folder);

            var request = new PutObjectRequest
            {
                BucketName = _s3Config.BucketName,
                Key = fileKey,
                InputStream = fileStream,
                ContentType = contentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            // Agregar metadatos
            request.Metadata.Add("uploaded-at", DateTime.UtcNow.ToString("O"));
            request.Metadata.Add("original-filename", fileName);

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);

            var publicUrl = GetPublicUrl(fileKey);

            _logger.LogInformation("Archivo subido exitosamente a S3 desde stream. Key: {FileKey}", fileKey);

            return publicUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir archivo {FileName} a S3 desde stream", fileName);
            throw;
        }
    }

    public async Task<string> UploadTextFileAsync(
        string textContent,
        string fileName,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(textContent))
        {
            throw new ArgumentException("El contenido de texto no puede estar vacío", nameof(textContent));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("El nombre del archivo es requerido", nameof(fileName));
        }

        var contentBytes = Encoding.UTF8.GetBytes(textContent);
        return await UploadFileAsync(contentBytes, fileName, "text/plain", folder, cancellationToken);
    }

    public async Task<bool> DeleteFileAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
        {
            throw new ArgumentException("La clave del archivo es requerida", nameof(fileKey));
        }

        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _s3Config.BucketName,
                Key = fileKey
            };

            await _s3Client.DeleteObjectAsync(request, cancellationToken);

            _logger.LogInformation("Archivo eliminado exitosamente de S3. Key: {FileKey}", fileKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar archivo {FileKey} de S3", fileKey);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
        {
            throw new ArgumentException("La clave del archivo es requerida", nameof(fileKey));
        }

        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _s3Config.BucketName,
                Key = fileKey
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar existencia del archivo {FileKey} en S3", fileKey);
            throw;
        }
    }

    public string GetPublicUrl(string fileKey, int? expirationHours = null)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
        {
            throw new ArgumentException("La clave del archivo es requerida", nameof(fileKey));
        }

        // Si se especifica una URL base personalizada (ej: CloudFront), usarla
        if (!string.IsNullOrWhiteSpace(_s3Config.PublicUrlBase))
        {
            return $"{_s3Config.PublicUrlBase.TrimEnd('/')}/{fileKey}";
        }

        // Generar URL presignada
        var expiration = expirationHours ?? _s3Config.DefaultExpirationHours;
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _s3Config.BucketName,
            Key = fileKey,
            Expires = DateTime.UtcNow.AddHours(expiration),
            Verb = HttpVerb.GET
        };

        try
        {
            return _s3Client.GetPreSignedURL(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar URL pública para archivo {FileKey}", fileKey);
            throw;
        }
    }

    /// <summary>
    /// Genera una clave única para el archivo en S3
    /// </summary>
    private string GenerateFileKey(string fileName, string? folder)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var sanitizedFileName = SanitizeFileName(fileName);

        var keyParts = new List<string> { _s3Config.BasePrefix, timestamp };

        if (!string.IsNullOrWhiteSpace(folder))
        {
            keyParts.Add(SanitizeFolderName(folder));
        }

        keyParts.Add($"{uniqueId}_{sanitizedFileName}");

        return string.Join("/", keyParts);
    }

    /// <summary>
    /// Sanitiza el nombre del archivo para evitar caracteres problemáticos
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = fileName;

        foreach (var invalidChar in invalidChars)
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }

        return sanitized;
    }

    /// <summary>
    /// Sanitiza el nombre de la carpeta para evitar caracteres problemáticos
    /// </summary>
    private static string SanitizeFolderName(string folderName)
    {
        var invalidChars = Path.GetInvalidPathChars();
        var sanitized = folderName;

        foreach (var invalidChar in invalidChars)
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }

        return sanitized.Replace('/', '_').Replace('\\', '_');
    }
}
