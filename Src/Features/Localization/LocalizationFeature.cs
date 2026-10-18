using System.Globalization;
using System.Resources;

namespace KeyLimiter.Features.Localization;

internal static class LocalizationFeature
{
    private static readonly ResourceManager Resources =
        global::KeyLimiter.Localization.strings.ResourceManager;

    internal static string Get(
        string name,
        params object[] arguments
    )
    {
        var culture = RDString.language.ToString() switch
        {
            "Korean" => CultureInfo.GetCultureInfo("ko"),
            _ => CultureInfo.GetCultureInfo("en")
        };

        var value = Resources.GetString(name, culture)
            ?? name;

        return arguments.Length == 0
            ? value
            : string.Format(culture, value, arguments);
    }
}
