using System.Text;

namespace eMarketing.Web.Utilities;

public static class DisplayTextNormalizer
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "-";

        string text = value.Trim();
        if (!LooksMojibake(text))
            return text;

        try
        {
            string decoded = Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-1").GetBytes(text));
            return LooksMojibake(decoded) ? text : decoded;
        }
        catch
        {
            return text;
        }
    }

    private static bool LooksMojibake(string value)
    {
        return value.Any(ch => ch is '\u00C3' or '\u00C4' or '\u00C5' or '\u00C2');
    }
}
