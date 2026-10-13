using Microsoft.AspNetCore.Http;

namespace mvc.Services.Interfaces;

public interface IFileService
{
    Task<string> SaveAsync(IFormFile file);
    Task DeleteAsync(string? filePath);
}
