using System.IO;

namespace ContosoDashboard.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageRoot;

    public LocalFileStorageService()
    {
        _storageRoot = Path.Combine(AppContext.BaseDirectory, "Uploads");
        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<string> SaveAsync(Stream content, string originalFileName, string contentType, CancellationToken cancellationToken = default)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new ArgumentException("Original file name is required.", nameof(originalFileName));
        }

        var safeExtension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid():N}{safeExtension}";
        var targetPath = Path.Combine(_storageRoot, storedFileName);

        await using var fileStream = File.Create(targetPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return storedFileName;
    }

    public Task<Stream> OpenReadAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storedFileName))
        {
            throw new ArgumentException("Stored file name is required.", nameof(storedFileName));
        }

        var targetPath = Path.Combine(_storageRoot, storedFileName);
        if (!File.Exists(targetPath))
        {
            throw new FileNotFoundException("Document file was not found.", targetPath);
        }

        var stream = File.OpenRead(targetPath);
        return Task.FromResult<Stream>(stream);
    }

    public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storedFileName))
        {
            return Task.CompletedTask;
        }

        var targetPath = Path.Combine(_storageRoot, storedFileName);
        if (File.Exists(targetPath))
        {
            File.Delete(targetPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storedFileName))
        {
            return Task.FromResult(false);
        }

        var targetPath = Path.Combine(_storageRoot, storedFileName);
        return Task.FromResult(File.Exists(targetPath));
    }
}
