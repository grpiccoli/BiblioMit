using BiblioMit.Extensions;
using System.Globalization;

namespace BiblioMit.Services
{
    public class InterceptProvider : IFormatProvider, ICustomFormatter
    {
        public object? GetFormat(Type? formatType)
        {
            if (formatType == typeof(ICustomFormatter))
            {
                return this;
            }
            else
            {
                return null;
            }
        }

        public string Format(string? format, object? arg, IFormatProvider? formatProvider)
        {
            // Display information about method call.
            if (!Equals(formatProvider))
            {
                return string.Empty;
            }
            // Set default format specifier             
            if (string.IsNullOrEmpty(format))
            {
                format = "N";
            }
            //string numericString = obj.ToString();
            if (arg is int && format.Equals("U", StringComparison.OrdinalIgnoreCase))
            {
                if (arg is null)
                {
                    return string.Empty;
                }

                string? argStr = arg.ToString();
                if (string.IsNullOrEmpty(argStr))
                {
                    return string.Empty;
                }

                string? formattedRut = argStr.RUTFormat();
                if (formattedRut is null)
                {
                    return string.Empty;
                }

                return formattedRut;
            }
            // If this is a byte and the "R" format string, format it with Roman numerals.
            if (arg is int @int && format.Equals("R", StringComparison.OrdinalIgnoreCase))
            {
                return @int.ToRomanNumeral();
            }
            // Use default for all other formatting.
            if (arg is IFormattable @formattable)
            {
                return @formattable.ToString(format, CultureInfo.CurrentCulture);
            }
            else
            {
                if (arg is null)
                {
                    return string.Empty;
                }

                string? argStr = arg.ToString();
                if (string.IsNullOrEmpty(argStr))
                {
                    return string.Empty;
                }

                return argStr;
            }
        }
    }
}
