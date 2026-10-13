using System.ComponentModel.DataAnnotations;
using mvc.Validation;

namespace mvc.BLL.DTOs;

// Текстові поля фільму однією мовою.
// ErrorMessage / Name — це ключі з Resources/SharedResource*.resx.
public class MovieTranslationDto
{
    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(10, ErrorMessage = "Validation_StringLength")]
    [Display(Name = "Field_Culture")]
    public string Culture { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(200, ErrorMessage = "Validation_StringLength")]
    [Display(Name = "Field_Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(100, ErrorMessage = "Validation_StringLength")]
    [NoDigits(ErrorMessage = "Validation_NoDigits")]
    [Display(Name = "Field_Director")]
    public string Director { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(50, ErrorMessage = "Validation_StringLength")]
    [Display(Name = "Field_Genre")]
    public string Genre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Validation_Required")]
    [StringLength(2000, ErrorMessage = "Validation_StringLength")]
    [Display(Name = "Field_Description")]
    public string Description { get; set; } = string.Empty;
}
