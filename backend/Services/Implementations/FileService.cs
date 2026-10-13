using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using mvc.Services.Interfaces;

namespace mvc.Services.Implementations;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private const string PostersFolder = "images/posters";

    public FileService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveAsync(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = Guid.NewGuid() + extension;
        var folder = Path.Combine(_environment.WebRootPath, PostersFolder.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(folder);
        var fullPath = Path.Combine(folder, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);
        return "/" + PostersFolder + "/" + fileName;
    }

    public Task DeleteAsync(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return Task.CompletedTask;
        var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
