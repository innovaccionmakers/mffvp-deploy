using Amazon.S3;
using Amazon.S3.Model;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Domain.Aws;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace Common.SharedKernel.Infrastructure.FileStorage;

public class S3FileStorageService(
    IAmazonS3 s3Client,
    IOptions<S3Config> s3Config,
    ILogger<S3FileStorageService> logger) : IFileStorageService
{
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
        if (fileSizeMB > s3Config.Value.MaxFileSizeMB)
        {
            throw new InvalidOperationException($"El archivo excede el tamaño máximo permitido de {s3Config.Value.MaxFileSizeMB}MB");
        }

        try
        {
            var fileKey = GenerateFileKey(fileName, folder);

            var request = new PutObjectRequest
            {
                BucketName = s3Config.Value.BucketName,
                Key = fileKey,
                InputStream = new MemoryStream(fileContent),
                ContentType = contentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            // Agregar metadatos
            request.Metadata.Add("uploaded-at", DateTime.UtcNow.ToString("O"));
            request.Metadata.Add("original-filename", fileName);
            request.Metadata.Add("file-size", fileContent.Length.ToString());

            var response = await s3Client.PutObjectAsync(request, cancellationToken);

            var publicUrl = GetPublicUrl(fileKey);

            logger.LogInformation("Archivo subido exitosamente a S3. Key: {FileKey}, Size: {FileSize} bytes",
                fileKey, fileContent.Length);

            return publicUrl;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al subir archivo {FileName} a S3", fileName);
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
                BucketName = s3Config.Value.BucketName,
                Key = fileKey,
                InputStream = fileStream,
                ContentType = contentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            // Agregar metadatos
            request.Metadata.Add("uploaded-at", DateTime.UtcNow.ToString("O"));
            request.Metadata.Add("original-filename", fileName);

            var response = await s3Client.PutObjectAsync(request, cancellationToken);

            var publicUrl = GetPublicUrl(fileKey);

            logger.LogInformation("Archivo subido exitosamente a S3 desde stream. Key: {FileKey}", fileKey);

            return publicUrl;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al subir archivo {FileName} a S3 desde stream", fileName);
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
                BucketName = s3Config.Value.BucketName,
                Key = fileKey
            };

            await s3Client.DeleteObjectAsync(request, cancellationToken);

            logger.LogInformation("Archivo eliminado exitosamente de S3. Key: {FileKey}", fileKey);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al eliminar archivo {FileKey} de S3", fileKey);
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
                BucketName = s3Config.Value.BucketName,
                Key = fileKey
            };

            await s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al verificar existencia del archivo {FileKey} en S3", fileKey);
            throw;
        }
    }

    public string GetPublicUrl(string fileKey, int? expirationHours = null)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
        {
            throw new ArgumentException("La clave del archivo es requerida", nameof(fileKey));
        }

        if (!string.IsNullOrWhiteSpace(s3Config.Value.PublicUrlBase))
        {
            return $"{s3Config.Value.PublicUrlBase.TrimEnd('/')}/{fileKey}";
        }

        // Generar URL presignada
        var expiration = expirationHours ?? s3Config.Value.DefaultExpirationHours;
        var request = new GetPreSignedUrlRequest
        {
            BucketName = s3Config.Value.BucketName,
            Key = fileKey,
            Expires = DateTime.UtcNow.AddHours(expiration),
            Verb = HttpVerb.GET
        };

        try
        {
            return s3Client.GetPreSignedURL(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al generar URL pública para archivo {FileKey}", fileKey);
            throw;
        }
    }

    private string GenerateFileKey(string fileName, string? folder)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var sanitizedFileName = SanitizeFileName(fileName);

        var keyParts = new List<string> { s3Config.Value.BasePrefix, timestamp };

        if (!string.IsNullOrWhiteSpace(folder))
        {
            keyParts.Add(SanitizeFolderName(folder));
        }

        keyParts.Add($"{uniqueId}_{sanitizedFileName}");

        return string.Join("/", keyParts);
    }


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
