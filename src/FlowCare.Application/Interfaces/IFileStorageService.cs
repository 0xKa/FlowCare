namespace FlowCare.Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Saves a file and returns the relative path.
    /// </summary>
    Task<string> SaveFileAsync(Stream stream, string folder, string fileName);

    /// <summary>
    /// Gets the full file path and content type. Returns null if file doesn't exist.
    /// </summary>
    (string FullPath, string ContentType)? GetFile(string relativePath);

    /// <summary>
    /// Validates that the file is an allowed image type (JPEG, PNG) by checking magic bytes.
    /// </summary>
    bool IsValidImage(Stream stream);

    /// <summary>
    /// Validates that the file is an allowed attachment type (JPEG, PNG, PDF) by checking magic bytes.
    /// </summary>
    bool IsValidAttachment(Stream stream);
}
