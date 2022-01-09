﻿using BiblioMit.Models;
using BiblioMit.Models.Entities.Digest;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
//using NCalc;
using System.Collections.Generic;
using System.Data;

namespace BiblioMit.Extensions
{
    public static class StringParsing
    {
        public static string? ParseAcronym(this string text)
        {
            string? noDiacritics = text?.ToUpperInvariant().RemoveDiacritics();
            if(!string.IsNullOrEmpty(noDiacritics)) return Regex.Replace(noDiacritics, @"[^A-Z]", "");
            return text;
        }
        public static double? ParseDouble(
            this string text, 
            int? decimalPlaces = null, 
            char? decimalSeparator = null, 
            bool? deleteAfter2ndNegative = null,
            string? operation = null)
        {
            if (string.IsNullOrEmpty(text)) return null;
            text = Regex.Replace(text, @"[^\d-\.,]", "");
            if (deleteAfter2ndNegative.HasValue && deleteAfter2ndNegative.Value) 
                text = Regex.Replace(text, @"([^^])\-.*", "$1");
            double? num = null;
            if (decimalSeparator.HasValue)
            {
                text = Regex.Replace(text, @$"[^\d\{decimalSeparator.Value}]", "");
                if (string.IsNullOrEmpty(text)) return null;
                var orders = text.Split(decimalSeparator.Value);
                if(orders.Length > 1)
                    num = double.Parse($"{string.Join("", orders.SkipLast(1))}.{orders.Last()}", CultureInfo.InvariantCulture);
                else
                    num = double.Parse($"{string.Join("", orders.First())}", CultureInfo.InvariantCulture);
            }
            else if (decimalPlaces.HasValue)
            {
                text = Regex.Replace(text, @"\D", "");
                if (string.IsNullOrEmpty(text)) return null;
                text = $"{text[..^decimalPlaces.Value]}.{text[^decimalPlaces.Value..]}";
                num = double.Parse(text, CultureInfo.InvariantCulture);
            }
            else
            {
                text = Regex.Replace(text, @"\.+", ".");
                text = Regex.Replace(text, @",+", ",");
                if (string.IsNullOrEmpty(text)) return null;
                bool negative = text.First() == '-';
                char sign = negative ? '-' : '+';
                var orders = Regex.Matches(text, @"[0-9]+").Select(m => m.Value);
                num = orders.Count() switch
                {
                    1 => double.Parse($"{sign}{orders.First()}", CultureInfo.InvariantCulture),
                    2 => SolveDouble(sign,orders.First(),orders.Last()),
                    _ => double.Parse($"{sign}{string.Join("",orders.SkipLast(1))}.{orders.Last()}", CultureInfo.InvariantCulture)
                };
            }
            if (operation != null)
            {
                DataTable dt = new();
                string? computed = dt.Compute($"{num}{operation}", null).ToString();
                bool parsed = double.TryParse(computed, NumberStyles.Float, CultureInfo.InvariantCulture, out double result);
                return parsed ? result : null;
            }
            return num;
        }
        public static double? SolveDouble(char sign, string head, string tail)
        {
            if (tail == null) return null;
            return tail.Length switch
            {
                3 => null,
                _ => double.Parse($"{sign}{head}.{tail}", CultureInfo.InvariantCulture)
            };
        }
        public static int? ParseInt(this string text, bool? deleteAfter2ndNegative = null, string? operation = null)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            text = Regex.Replace(text, @"[^\d-\.,]", "");
            if (deleteAfter2ndNegative.HasValue && deleteAfter2ndNegative.Value)
                text = Regex.Replace(text, @"([^^])\-.*", "$1");
            else
                text = Regex.Replace(text, @"([^^])\-+", "$1");
            text = Regex.Replace(text, @"[\.,].*", "");
            if (string.IsNullOrEmpty(text)) return null;
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
            var formats = new string[] { "yyyyMMdd", "yyyy-MM-dd", "dd-MM-yyyy HH:mm", "dd-MM-yyyy", "dd-MM-yyyy HH:mm'&nbsp;'", "dd-MM-yyyy HH:mm 'HRS.'" };
            var parsed = DateTime.TryParseExact(text, formats, new CultureInfo("es-CL"), DateTimeStyles.None, out DateTime date);
            if (parsed) return date;
            return null;
        }
        public static ProductionType? ParseProductionType(this string text)
        {
            if (text == null) return 0;
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
            if (text == null) return null;
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
