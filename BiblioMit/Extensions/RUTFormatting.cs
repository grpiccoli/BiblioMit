﻿using System.Globalization;

namespace BiblioMit.Extensions
{
    public static class RUTFormatting
    {
        public static string RUTGetDigit(this int Id)
        {
            int Digito, Contador = 2, Multiplo, Acumulador = 0;
            while (Id != 0)
            {
                Multiplo = (Id % 10) * Contador;
                Acumulador += Multiplo;
                Id /= 10;
                Contador += 1;
                if (Contador == 8)
                    Contador = 2;
            }
            Digito = 11 - (Acumulador % 11);
            return Digito switch
            {
                10 => "K",
                11 => "0",
                _ => Digito.ToString(CultureInfo.InvariantCulture)
            };
        }
        public static string? RUTFormat(this string? rut, bool thousandSep = true, bool dash = true)
        {
            if (string.IsNullOrWhiteSpace(rut)) return null;
            (int rut, string dv)? ruT = RUTUnformat(rut);
            if (ruT is null) return null;
            return RUTFormat(ruT.Value.rut, thousandSep, dash);
        }
        public static string RUTFormat(this int rut, bool thousandSep = true, bool dash = true) =>
            $@"{(thousandSep ?
                rut.ToString("N0", new CultureInfo("es-CL")) :
                rut.ToString(CultureInfo.InvariantCulture))}{(dash ? "-" : "")}{RUTGetDigit(rut)}";
        public static string RUTFonasa(this int rut) =>
            $"{rut.ToString("D10", CultureInfo.InvariantCulture)}-{RUTGetDigit(rut)}";
        public static string? RUTFonasa(this string rut)
        {
            (int rut, string dv)? unformated = RUTUnformat(rut);
            if (unformated is null) return null;
            return RUTFonasa(unformated.Value.rut);
        }
        public static bool RUTIsValid(this int rut, string dv)
        {
            string Dv = RUTGetDigit(rut);
            return Dv == dv;
        }
        public static bool IsValid(this string rut) => RUTUnformat(rut) != null;
        public static (int rut, string dv)? RUTUnformat(this string formatted)
        {
            string[] array = formatted.Replace(".", "", StringComparison.InvariantCulture).Split("-");
            bool parsed = int.TryParse(array[0], out int rut);
            if (parsed)
            {
                if (RUTGetDigit(rut) == array[1].ToUpper(new CultureInfo("es-CL"))) return (rut, dv: array[1]);
            }
            return null;
        }
    }
}
