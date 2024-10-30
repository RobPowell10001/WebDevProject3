using System.ComponentModel.DataAnnotations;

namespace Attributes;

public class IMDBLinkValidationAttribute : ValidationAttribute
{
    private readonly string _requiredword;

    public IMDBLinkValidationAttribute()
    {
        _requiredword = "imdb.com";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || !(value is string inputString))
        {
            return new ValidationResult("Input is required");
        }

        if (!inputString.Contains(_requiredword, StringComparison.OrdinalIgnoreCase))
        {
            return new ValidationResult("Input must be an IMDB link!");
        }
        return ValidationResult.Success;
    }
}
