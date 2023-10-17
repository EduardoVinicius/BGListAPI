using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BGList.Attributes
{
    public class LettersOnlyValidatorAttribute : ValidationAttribute
    {
        public bool UseRegex { get; set; }

        public LettersOnlyValidatorAttribute(bool useRegex = false) : base("Value must contain only letter (no spaces digits or other chars).")
        {
            UseRegex = useRegex;
        }

        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext)
        {
            var strValue = value as string;

            if (!string.IsNullOrEmpty(strValue)
                && ((UseRegex && Regex.IsMatch(strValue!, "^[A-Za-z]+$"))
                    || strValue.All(Char.IsLetter))
                )
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }
}
