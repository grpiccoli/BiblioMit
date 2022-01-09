using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

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
                var file = string.Join('.', typeof(T).Namespace?.Split('.').Skip(1)
                    .Append(typeof(T).Name).Append(lang).Append("resx"));
                XmlReaderSettings settings = new()
                {
                    IgnoreWhitespace = true
                };
                XmlDocument document = new();
                document.Load($"Resources/{file}");
                XmlNamespaceManager m = new(document.NameTable);
                var element = document.SelectSingleNode($"ns:data[name='{text}']/ns:value", m);
                return element == null ? text : element.InnerText;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
                return text;
            }
        }
    }
}
