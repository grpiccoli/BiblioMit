using System.Globalization;

namespace BiblioMit.Extensions
{
    public static class Statics
    {
        public const string DefaultCulture = "en";
        public static readonly CultureInfo[] SupportedCultures = new[]
        {
            new CultureInfo(DefaultCulture),
            new CultureInfo("es")
        };
    }
}
