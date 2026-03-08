using FlowCare.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace FlowCare.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    // Magic bytes for file type detection
    private static readonly byte[] JpegMagic = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngMagic = [0x89, 0x50, 0x4E, 0x47];
    private static readonly byte[] PdfMagic = [0x25, 0x50, 0x44, 0x46]; // %PDF

    public LocalFileStorageService(IConfiguration configuration, IWebHostEnvironment env)
    {
        var configuredPath = configuration["FileStorage:BasePath"] ?? "uploads";

        _basePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(env.ContentRootPath, configuredPath);
    }
    public async Task<string> SaveFileAsync(Stream stream, string folder, string fileName)
    {
        var dir = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(dir);

        var fullPath = Path.Combine(dir, fileName);
        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream);

        // Return relative path for DB storage
        return Path.Combine(folder, fileName);
    }

    public (string FullPath, string ContentType)? GetFile(string relativePath)
    {
        var fullPath = Path.Combine(_basePath, relativePath);

        // Prevent path traversal
        var normalizedFull = Path.GetFullPath(fullPath);
        var normalizedBase = Path.GetFullPath(_basePath);
        if (!normalizedFull.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase))
            return null;

        if (!File.Exists(fullPath))
            return null;

        var contentType = GetContentType(fullPath);
        return (fullPath, contentType);
    }

    public bool IsValidImage(Stream stream)
    {
        return HasMagicBytes(stream, JpegMagic) || HasMagicBytes(stream, PngMagic);
    }

    public bool IsValidAttachment(Stream stream)
    {
        return HasMagicBytes(stream, JpegMagic)
            || HasMagicBytes(stream, PngMagic)
            || HasMagicBytes(stream, PdfMagic);
    }

    private static bool HasMagicBytes(Stream stream, byte[] magic)
    {
        if (stream.Length < magic.Length)
            return false;

        var originalPosition = stream.Position;
        stream.Position = 0;

        var buffer = new byte[magic.Length];
        var bytesRead = stream.Read(buffer, 0, magic.Length);
        stream.Position = originalPosition;

        if (bytesRead < magic.Length)
            return false;

        return buffer.AsSpan().SequenceEqual(magic);
    }

    private static string GetContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
