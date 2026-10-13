using Microsoft.AspNetCore.Http;
using mvc.BLL.DTOs;

namespace mvc.BLL.Interfaces;

public interface IMovieService
{
    Task<List<MovieDto>> GetAllAsync();
    Task<MovieDto?> GetByIdAsync(int id);
    Task<MovieEditDto?> GetForEditAsync(int id);
    Task<int> CreateAsync(MovieCreateDto dto, IFormFile? posterFile);
    Task<bool> UpdateAsync(int id, MovieUpdateDto dto, IFormFile? posterFile);
    Task<bool> DeleteAsync(int id);
    string? ValidatePosterFile(IFormFile? posterFile);
}
