using System.Globalization;
using System.Xml;

namespace BiblioMit.Extensions
{
    public static class ResXtensions
    {
        public static string LocalizeString<T>(this T t, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException($"{t} text is null");
            }
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (lang == "en") return text;
            try
            {
                string? nameSpace = typeof(T).Namespace;
                if (nameSpace != null)
                {
                    string file = string.Join('.', nameSpace.Split('.').Skip(1)
                        .Append(typeof(T).Name).Append(lang).Append("resx"));
                    XmlReaderSettings settings = new()
                    {
                        IgnoreWhitespace = true
                    };
                    XmlDocument document = new();
                    document.Load($"Resources/{file}");
                    XmlNamespaceManager m = new(document.NameTable);
                    XmlNode? element = document.SelectSingleNode($"ns:data[name='{text}']/ns:value", m);
                    return element == null ? text : element.InnerText;
                }
                return text;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
                return text;
            }
        }
    }
}
