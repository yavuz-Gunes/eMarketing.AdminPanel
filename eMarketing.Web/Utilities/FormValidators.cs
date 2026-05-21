using System.Text.RegularExpressions;

namespace eMarketing.Web.Utilities;

public static partial class FormValidators
{
    public const int DefaultPhoneMinDigits = 10;
    public const int DefaultPhoneMaxDigits = 12;

    public static bool HasValue(string? value) => !string.IsNullOrWhiteSpace(value);

    public static int DigitCount(string? value) => string.IsNullOrWhiteSpace(value) ? 0 : value.Count(char.IsDigit);

    public static bool IsValidEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        return EmailRegex().IsMatch(value.Trim());
    }

    public static bool IsValidPhone(string? value, int minDigits = DefaultPhoneMinDigits, int maxDigits = DefaultPhoneMaxDigits)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        int digits = DigitCount(value);
        return digits >= minDigits && digits <= maxDigits;
    }

    public static bool IsDigitsOnly(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        return value.All(char.IsDigit);
    }

    public static bool HasNoDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        return value.All(character => !char.IsDigit(character));
    }

    public static string? RequiredError(string? value, string label)
        => HasValue(value) ? null : $"{label} zorunludur.";

    public static string? EmailError(string? value)
        => IsValidEmail(value) ? null : "Geçerli bir e-posta adresi girin.";

    public static string? PhoneError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        int digits = DigitCount(value);
        if (digits < DefaultPhoneMinDigits)
            return "Telefon numarası en az 10 rakam olmalı.";

        if (digits > DefaultPhoneMaxDigits)
            return "Telefon numarası çok uzun.";

        return null;
    }

    public static string? MaxDigitsError(string? value, int maxDigits, string label)
        => DigitCount(value) <= maxDigits ? null : $"{label} en fazla {maxDigits} rakam olmalı.";

    public static string? PersonError(string? value, string label)
        => HasNoDigits(value) ? null : $"{label} alanına sayı girilemez.";

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
