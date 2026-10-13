using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace mvc.BLL.DTOs;

public class MovieCreateDto
{
    [Required(ErrorMessage = "Validation_Required")]
    [Range(1888, 2100, ErrorMessage = "Validation_Range")]
    [Display(Name = "Field_ReleaseYear")]
    public int ReleaseYear { get; set; }

    [Display(Name = "Field_Poster")]
    public IFormFile? Poster { get; set; }

    // Переклади, заповнені користувачем (щонайменше один).
    // У multipart/form-data: Translations[0].Culture, Translations[0].Title, ...
    public List<MovieTranslationDto> Translations { get; set; } = new();
}
