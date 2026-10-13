using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using mvc.BLL.DTOs;
using mvc.BLL.Interfaces;
using mvc.DAL;
using mvc.DAL.Interfaces;
using mvc.Localization;
using mvc.Resources;
using mvc.Services.Interfaces;

namespace mvc.BLL.Services;

public class MovieService : IMovieService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxPosterSizeBytes = 5 * 1024 * 1024;

    public MovieService(IUnitOfWork unitOfWork, IFileService fileService, IStringLocalizer<SharedResource> localizer)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _localizer = localizer;
    }

    // Мова поточного запиту ("uk", "en", "ru") — її виставляє RequestLocalizationMiddleware
    private static string CurrentCulture() => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

    public async Task<List<MovieDto>> GetAllAsync()
    {
        var culture = CurrentCulture();
        return (await _unitOfWork.Movies.GetAllAsync()).Select(m => ToDto(m, culture)).ToList();
    }

    public async Task<MovieDto?> GetByIdAsync(int id)
    {
        var movie = await _unitOfWork.Movies.GetByIdAsync(id);
        return movie == null ? null : ToDto(movie, CurrentCulture());
    }

    public async Task<MovieEditDto?> GetForEditAsync(int id)
    {
        var movie = await _unitOfWork.Movies.GetByIdAsync(id);
        if (movie == null) return null;

        return new MovieEditDto
        {
            Id = movie.Id,
            ReleaseYear = movie.ReleaseYear,
            PosterPath = movie.PosterPath,
            Translations = movie.Translations
                .OrderBy(t => Array.IndexOf(AppCultures.Supported, t.Culture))
                .Select(t => new MovieTranslationDto
                {
                    Culture = t.Culture,
                    Title = t.Title,
                    Director = t.Director,
                    Genre = t.Genre,
                    Description = t.Description
                })
                .ToList()
        };
    }

    public async Task<int> CreateAsync(MovieCreateDto dto, IFormFile? posterFile)
    {
        var translations = ValidateMovie(dto);
        await EnsureTitlesAreUniqueAsync(0, translations);

        var movie = new Movie { ReleaseYear = dto.ReleaseYear };
        foreach (var t in translations)
            movie.Translations.Add(ToEntity(t));

        if (posterFile != null && posterFile.Length > 0)
            movie.PosterPath = await _fileService.SaveAsync(posterFile);

        _unitOfWork.Movies.Add(movie);
        await _unitOfWork.SaveChangesAsync();
        return movie.Id;
    }

    public async Task<bool> UpdateAsync(int id, MovieUpdateDto dto, IFormFile? posterFile)
    {
        var translations = ValidateMovie(dto);
        var movie = await _unitOfWork.Movies.GetByIdAsync(id);
        if (movie == null) return false;

        await EnsureTitlesAreUniqueAsync(id, translations);

        var oldPoster = movie.PosterPath;
        movie.ReleaseYear = dto.ReleaseYear;

        // Мови, яких немає у формі, видаляються; наявні оновлюються; нові додаються
        foreach (var removed in movie.Translations.Where(e => translations.All(t => t.Culture != e.Culture)).ToList())
            movie.Translations.Remove(removed);

        foreach (var t in translations)
        {
            var existing = movie.Translations.FirstOrDefault(e => e.Culture == t.Culture);
            if (existing == null)
            {
                movie.Translations.Add(ToEntity(t));
            }
            else
            {
                existing.Title = t.Title;
                existing.Director = t.Director;
                existing.Genre = t.Genre;
                existing.Description = t.Description;
            }
        }

        string? newPoster = null;
        if (posterFile != null && posterFile.Length > 0)
        {
            newPoster = await _fileService.SaveAsync(posterFile);
            movie.PosterPath = newPoster;
        }

        _unitOfWork.Movies.Update(movie);
        await _unitOfWork.SaveChangesAsync();

        if (newPoster != null) await _fileService.DeleteAsync(oldPoster);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var movie = await _unitOfWork.Movies.GetByIdAsync(id);
        if (movie == null) return false;
        var poster = movie.PosterPath;
        _unitOfWork.Movies.Delete(movie); // переклади видаляються каскадно
        await _unitOfWork.SaveChangesAsync();
        await _fileService.DeleteAsync(poster);
        return true;
    }

    public string? ValidatePosterFile(IFormFile? posterFile)
    {
        if (posterFile == null || posterFile.Length == 0) return null;
        if (posterFile.Length > MaxPosterSizeBytes) return _localizer["Error_PosterTooLarge"].Value;
        var extension = Path.GetExtension(posterFile.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension)) return _localizer["Error_PosterFormat"].Value;
        if (!posterFile.ContentType.StartsWith("image/")) return _localizer["Error_PosterNotImage"].Value;
        return null;
    }

    // Перевіряє дані та повертає нормалізований список перекладів (культура — у нижньому регістрі)
    private List<MovieTranslationDto> ValidateMovie(MovieCreateDto dto)
    {
        if (dto.ReleaseYear < 1888 || dto.ReleaseYear > DateTime.Now.Year)
            throw new ArgumentException(_localizer["Error_InvalidYear"].Value);

        var translations = dto.Translations ?? new List<MovieTranslationDto>();
        if (translations.Count == 0)
            throw new ArgumentException(_localizer["Error_NoTranslations"].Value);

        var seen = new HashSet<string>();
        foreach (var t in translations)
        {
            t.Culture = (t.Culture ?? string.Empty).Trim().ToLowerInvariant();

            if (!AppCultures.IsSupported(t.Culture))
                throw new ArgumentException(_localizer["Error_UnsupportedCulture", t.Culture].Value);
            if (!seen.Add(t.Culture))
                throw new ArgumentException(_localizer["Error_DuplicateCulture", t.Culture].Value);
            if (string.IsNullOrWhiteSpace(t.Title))
                throw new ArgumentException(_localizer["Error_TitleEmpty"].Value);
            if (t.Description.Length > 2000)
                throw new ArgumentException(_localizer["Error_DescriptionTooLong"].Value);
        }

        return translations;
    }

    // Назва має бути унікальною в межах однієї мови
    private async Task EnsureTitlesAreUniqueAsync(int movieId, List<MovieTranslationDto> translations)
    {
        foreach (var t in translations)
        {
            var culture = t.Culture;
            var title = t.Title;
            if (await _unitOfWork.Movies.AnyAsync(m =>
                    m.Id != movieId && m.Translations.Any(x => x.Culture == culture && x.Title == title)))
                throw new InvalidOperationException(_localizer["Error_DuplicateTitle"].Value);
        }
    }

    private static MovieTranslation ToEntity(MovieTranslationDto t) => new()
    {
        Culture = t.Culture,
        Title = t.Title,
        Director = t.Director,
        Genre = t.Genre,
        Description = t.Description
    };

    // Вибір перекладу: запитана мова -> мова за замовчуванням -> будь-який наявний
    private static MovieTranslation? PickTranslation(Movie movie, string culture) =>
        movie.Translations.FirstOrDefault(t => t.Culture == culture)
        ?? movie.Translations.FirstOrDefault(t => t.Culture == AppCultures.Default)
        ?? movie.Translations.OrderBy(t => t.Id).FirstOrDefault();

    private static MovieDto ToDto(Movie movie, string culture)
    {
        var translation = PickTranslation(movie, culture);
        return new MovieDto
        {
            Id = movie.Id,
            ReleaseYear = movie.ReleaseYear,
            PosterPath = movie.PosterPath,
            Title = translation?.Title ?? string.Empty,
            Director = translation?.Director ?? string.Empty,
            Genre = translation?.Genre ?? string.Empty,
            Description = translation?.Description ?? string.Empty,
            Culture = translation?.Culture ?? culture,
            AvailableCultures = movie.Translations
                .Select(t => t.Culture)
                .OrderBy(c => Array.IndexOf(AppCultures.Supported, c))
                .ToList()
        };
    }
}
