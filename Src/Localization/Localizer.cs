using System.Globalization;
using System.Resources;

namespace KeyLimiter.Localization;

internal static class Localizer
{
    private static readonly ResourceManager Resources =
        strings.ResourceManager;

    internal static string Get(
        string key,
        params object[] arguments
    )
    {
        var culture = RDString.language.ToString() switch
        {
            "Korean" => CultureInfo.GetCultureInfo("ko"),
            _ => CultureInfo.GetCultureInfo("en")
        };

        var value = Resources.GetString(key, culture)
            ?? key;

        return arguments.Length == 0
            ? value
            : string.Format(culture, value, arguments);
    }
}
