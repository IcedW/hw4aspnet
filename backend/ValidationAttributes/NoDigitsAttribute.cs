using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using mvc.Resources;

namespace mvc.Validation
{
    // Custom validation attribute: text must not contain digits.
    // ErrorMessage — ключ ресурсу (Validation_NoDigits), повідомлення локалізується
    // мовою поточного запиту. IClientModelValidator залишено для сумісності з
    // jQuery Unobtrusive, див. wwwroot/js/custom-validation.js.
    public class NoDigitsAttribute : ValidationAttribute, IClientModelValidator
    {
        private const string DefaultMessageKey = "Validation_NoDigits";

        public NoDigitsAttribute()
        {
            ErrorMessage = DefaultMessageKey;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            var text = value as string;
            if (!string.IsNullOrEmpty(text) && text.Any(char.IsDigit))
            {
                var localizer = context.GetService(typeof(IStringLocalizer<SharedResource>)) as IStringLocalizer<SharedResource>;
                return new ValidationResult(Format(localizer, context.DisplayName));
            }
            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            var localizer = context.ActionContext.HttpContext.RequestServices.GetService<IStringLocalizer<SharedResource>>();
            context.Attributes["data-val"] = "true";
            context.Attributes["data-val-nodigits"] = Format(localizer, context.ModelMetadata.GetDisplayName());
        }

        private string Format(IStringLocalizer? localizer, string displayName)
        {
            var key = ErrorMessage ?? DefaultMessageKey;
            return localizer != null ? localizer[key, displayName].Value : string.Format(key, displayName);
        }
    }
}
