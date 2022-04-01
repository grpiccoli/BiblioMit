using BiblioMit.Models;
using BiblioMit.Models.Entities.Digest;
//using NCalc;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BiblioMit.Extensions
{
    public static class StringParsing
    {
        public static string? ParseAcronym(this string text)
        {
            string? noDiacritics = text?.ToUpperInvariant().RemoveDiacritics();
            if (!string.IsNullOrEmpty(noDiacritics))
            {
                return Regex.Replace(noDiacritics, @"[^A-Z]", "");
            }

            return text;
        }
        public static double? ParseDouble(
            this string text,
            string? operation = null)
        {
            text = Regex.Replace(text, @"[^\d-\.,]", "");
            text = Regex.Replace(text, @"([^^])\-.*", "$1");
            text = Regex.Replace(text, @"\.+", ".");
            text = Regex.Replace(text, @",+", ",");

            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            bool parsed = double.TryParse(text, NumberStyles.Any, new CultureInfo("es-CL"), out double num);
            if (operation != null)
            {
                DataTable dt = new();
                string? computed = dt.Compute($"{num}{operation}", null).ToString();
                bool opparsed = double.TryParse(computed, NumberStyles.Float, CultureInfo.InvariantCulture, out double result);
                return opparsed ? result : null;
            }
            return parsed ? num : null;
        }
        public static double? SolveDouble(char sign, string head, string tail)
        {
            if (tail == null)
            {
                return null;
            }

            return tail.Length switch
            {
                3 => null,
                _ => double.Parse($"{sign}{head}.{tail}", CultureInfo.InvariantCulture)
            };
        }
        public static int? ParseInt(this string text, bool? deleteAfter2ndNegative = null, string? operation = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            text = Regex.Replace(text, @"[^\d-\.,]", "");
            if (deleteAfter2ndNegative.HasValue && deleteAfter2ndNegative.Value)
            {
                text = Regex.Replace(text, @"([^^])\-.*", "$1");
            }
            else
            {
                text = Regex.Replace(text, @"([^^])\-+", "$1");
            }

            text = Regex.Replace(text, @"[\.,].*", "");
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            if (operation != null)
            {
                DataTable dt = new();
                string? computed = dt.Compute($"{text}{operation}", null).ToString();
                bool parsed = int.TryParse(computed, NumberStyles.Float, CultureInfo.InvariantCulture, out int result);
                return parsed ? result : null;
            }
            return int.Parse(text, CultureInfo.InvariantCulture);
        }
        public static DateTime? ParseDateTime(this string text)
        {
            string[] formats = new string[] { "yyyyMMdd", "yyyy-MM-dd", "dd-MM-yyyy HH:mm", "dd-MM-yyyy", "dd-MM-yyyy HH:mm'&nbsp;'", "dd-MM-yyyy HH:mm 'HRS.'" };
            bool parsed = DateTime.TryParseExact(text, formats, new CultureInfo("es-CL"), DateTimeStyles.None, out DateTime date);
            if (parsed)
            {
                return date;
            }

            return null;
        }
        public static ProductionType? ParseProductionType(this string text)
        {
            if (text == null)
            {
                return 0;
            }

            IEnumerable<string> tipos = ProductionType.Unknown.GetNamesList();
            for (int i = 1; i < tipos.Count(); i++)
            {
                if (text.Contains(tipos.ElementAt(i), StringComparison.Ordinal))
                {
                    return (ProductionType)i;
                }
            }
            return ProductionType.Unknown;
        }
        public static Item? ParseItem(this string text)
        {
            if (text == null)
            {
                return null;
            }

            IEnumerable<string> tipos = Item.Product.GetNamesList();
            for (int i = 0; i < tipos.Count(); i++)
            {
                if (text[0].Equals(tipos.ElementAt(i)[0]))
                {
                    return (Item)i;
                }
            }
            return null;
        }
    }
}
