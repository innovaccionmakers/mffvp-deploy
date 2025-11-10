namespace Common.SharedKernel.Application.Abstractions;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(
        byte[] fileContent,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default);
    Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default);
    Task<string> UploadTextFileAsync(
        string textContent,
        string fileName,
        string? folder = null,
        CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string fileKey, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string fileKey, CancellationToken cancellationToken = default);
    string GetPublicUrl(string fileKey, int? expirationHours = null, string? fileName = null);
}
